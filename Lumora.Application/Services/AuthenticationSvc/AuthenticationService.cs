namespace Lumora.Application.Services.Authentication
{
    public class AuthenticationService(
        IMapper mapper,
        IIdentityRepository identityService,
        ITokenRepository tokenRepository,
        IRoleService roleService,
        IUserService userService,
        ITokenService tokenService,
        AuthenticationMessage messages,
        IOptions<JwtConfig> jwtConfig,
        IUnitOfWork unitOfWork,
        RoleMessages roleMessages,
        IPermissionService permissionService,
        ILogger<AuthenticationService> logger) : IAuthenticationService
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly ITokenRepository _tokenRepository = tokenRepository;
        private readonly IOptions<JwtConfig> _jwtConfig = jwtConfig;
        private readonly IPermissionService _permissionService = permissionService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IRoleService _roleService = roleService;
        private readonly ILogger<AuthenticationService> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IIdentityRepository _identityService = identityService;

        /// <inheritdoc/>
        public async Task<GeneralResult> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var isAnyUser = await userService.ExsistByPhoneNumberAsync(dto.PhoneNumber);
                if (isAnyUser)
                {
                    return new GeneralResult(false, messages.MsgPhoneNumberNotAvilable, null, ErrorType.BadRequest);
                }

                if (dto.Password != dto.ConfirmPassword)
                {
                    return new GeneralResult(false, messages.MsgPasswordNotMatch, null, ErrorType.BadRequest);
                }

                var newUser = _mapper.Map<User>(dto);
                newUser.UserName = dto.PhoneNumber;
                newUser.PhoneNumberConfirmed = true; // temp

                var createResult = await _identityService.CreateAsync(newUser, dto.Password!);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors);
                    _logger.LogError("Error creating user with phoneNumber={PhoneNumber} - {Errors}", dto.PhoneNumber, errors);
                    return new GeneralResult(false, messages.MsgUserRegistrationFailed, null, ErrorType.BadRequest);
                }

                if ((await _roleService.IsUserInRoleAsync(newUser.Id, AppRoles.User, cancellationToken)).Data == false)
                {
                    if ((await _roleService.AssignRoleAsync(newUser.Id, AppRoles.User)).IsSuccess == false)
                    {
                        _logger.LogWarning("RegisterAsync - Failed to assign User role. UserId: {UserId}", newUser.Id);
                        await transaction.RollbackAsync(cancellationToken);
                        return new GeneralResult(false, roleMessages.MsgAssignRoleFailed, null, ErrorType.InternalServerError);
                    }
                }

                // TODO: Send OTP

                //try
                //{
                //    var (otp, expireAt) = await _otpService.GenerateAndSendOtpAsync(newUser.Id, dto.PhoneNumber!);
                //    _logger.LogInformation("OTP {Otp} sent to {PhoneNumber} and will expire at {ExpireAt}", otp, dto.PhoneNumber, expireAt);
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogError(ex, "Failed to send OTP to {PhoneNumber}.", dto.PhoneNumber);
                //    throw new InvalidOperationException("Failed to send OTP. Please try again.");
                //}

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var user = new UserProfileDto
                {
                    Id = newUser.Id,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    City = dto.City,
                    Sex = dto.Sex
                };

                _logger.LogInformation("User {UserId} created successfully.", newUser.Id);
                return new GeneralResult(true, messages.MsgUserRegistered, user, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error registering user with phoneNumber={PhoneNumber}", dto.PhoneNumber);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage("register user."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserWithoutPhoneNumberConfirmedAsync(cancellationToken, dto.PhoneNumber, null);
                if (user.Data == null)
                {
                    _logger.LogWarning("LoginWith2FACode : User not found or deleted or inactive. PhoneNumber={PhoneNumber}", dto.PhoneNumber);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                if (!user.Data.PhoneNumberConfirmed)
                {
                    _logger.LogInformation("Phone number is not confirmed for user {PhoneNumber}.", dto.PhoneNumber);
                    return new GeneralResult(false, messages.MsgPhoneNotConfirmed, null, ErrorType.BadRequest);

                    // TODO: Send SMS confirmation  
                }

                var signResult = await _identityService.CheckPasswordAsync(user.Data, dto.Password);
                if (!signResult.Succeeded)
                {
                    if (signResult.IsLockedOut)
                    {
                        _logger.LogWarning("Too many requests for user {PhoneNumber}.", dto.PhoneNumber);
                        return new GeneralResult(false, messages.MsgTooManyRequests, null, ErrorType.BadRequest);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid credentials for user {PhoneNumber}.", dto.PhoneNumber);
                        return new GeneralResult(false, messages.MsgInvalidCredentials, null, ErrorType.BadRequest);
                    }
                }

                if (await _identityService.GetTwoFactorEnabledAsync(user.Data))
                {
                    _logger.LogInformation("2FA is enabled for user {PhoneNumber}. Awaiting verification code.", dto.PhoneNumber);
                    return new GeneralResult(false, messages.MsgTwoFACodeRequired, new { RequiresTwoFactor = true, UserId = user.Data.Id });
                }

                var tokenResult = await IssueTokensAsync(user.Data);
                if (tokenResult.IsSuccess == false) return tokenResult;

                var tokenDto = (TokenDto)tokenResult.Data!;
                _logger.LogInformation("Login successful for user {PhoneNumber}.", dto.PhoneNumber);
                return new GeneralResult(true, messages.MsgLoginSuccessful, tokenDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for PhoneNumber {PhoneNumber}.", dto.PhoneNumber);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" login"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> LoginWith2FACodeAsync(Login2FADto dto, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserAsync(cancellationToken, null, null, dto.UserId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("LoginWith2FACode : User not found or deleted or inactive. ID={userId}", dto.UserId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                if (!await _identityService.GetTwoFactorEnabledAsync(user.Data))
                {
                    _logger.LogWarning("2FA login attempt for user without 2FA enabled. UserId: {UserId}", dto.UserId);
                    return new GeneralResult(false, messages.MsgTwoFANotEnabled, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(dto.VerificationCode))
                {
                    _logger.LogWarning("Verification code is required for 2FA login. UserId: {UserId}", dto.UserId);
                    return new GeneralResult(false, messages.MsgVerificationCodeRequired, null, ErrorType.BadRequest);
                }

                var isValid = await VerifyTwoFactorCodeAsync(user.Data, dto.VerificationCode);
                if (isValid.IsSuccess == false)
                {
                    _logger.LogWarning("Invalid 2FA code for user {UserId}.", dto.UserId);
                    return new GeneralResult(false, isValid.Message ?? messages.MsgInvalidVerificationCode, null, isValid.ErrorType);
                }

                // Code is valid → generate tokens
                var tokenResult = await IssueTokensAsync(user.Data);
                if (tokenResult.IsSuccess == false) return tokenResult;

                var tokenDto = (TokenDto)tokenResult.Data!;
                _logger.LogInformation("Login successful for user {PhoneNumber}.", user.Data.PhoneNumber);
                return new GeneralResult(true, messages.MsgLoginSuccessful, tokenDto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during 2FA login for user {UserId}.", dto.UserId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" login with 2FA."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> LogoutAsync(string userId, string refreshToken, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("logout : User not found or deleted or inactive. ID={userId}", userId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                // Revoke the Refresh Token sent.
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    var hashed = _tokenService.HashRefreshToken(refreshToken);
                    var token = await _tokenRepository.GetActiveRefreshTokenAsync(userId, hashed, cancellationToken);

                    if (token != null)
                    {
                        token.IsUsed = true;
                        token.IsRevoked = true;
                        _tokenRepository.UpdateRefreshToken(token);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }

                // If you have Cookie/Session authentication
                // (e.g. when using SignInManager)
                await _identityService.SignOutAsync();
                return new GeneralResult(true, messages.MsgLogoutSuccessful, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Logout for userId={UserId}", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" logout"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> EnableTwoFactorAuthAsync(Enable2FADto dto, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserAsync(cancellationToken, null, null, dto.UserId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("EnableTwoFactorAuthAsync: User not found or deleted or inactive. ID={UserId}", dto.UserId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                // Verify the token from authenticator app
                if (dto.AppVerificationCode is null)
                {
                    _logger.LogWarning("EnableTwoFactorAuthAsync: App verification code is missing.");
                    return new GeneralResult(false, messages.MsgAppVerificationCodeMissing, null, ErrorType.BadRequest);
                }

                // Verify the token from authenticator app.
                var isTokenValid = await VerifyTwoFactorCodeAsync(user.Data, dto.AppVerificationCode);
                if (isTokenValid.IsSuccess == false)
                {
                    return new GeneralResult(false, messages.MsgInvalid2FAToken, null, ErrorType.BadRequest);
                }

                var result = await _identityService.SetTwoFactorEnabledAsync(user.Data, true);
                return result
                    ? new GeneralResult(true, messages.MsgTwoFAEnabled, null, ErrorType.Success)
                    : new GeneralResult(false, messages.MsgTwoFAEnableFailed, null, ErrorType.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while enabling 2FA for user {UserId}", dto.UserId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" enable two factor auth"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DisableTwoFactorAuthAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("DisableTwoFactorAuthAsync: User not found or deleted or inactive. ID={UserId}", userId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                var result = await _identityService.SetTwoFactorEnabledAsync(user.Data, false);
                return result
                    ? new GeneralResult(true, messages.MsgTwoFADisabled, null, ErrorType.Success)
                    : new GeneralResult(false, messages.MsgTwoFADisableFailed, null, ErrorType.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DisableTwoFactorAuthAsync - Exception occurred while disabling 2FA for user {UserId}", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" disable 2FA"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> GetTwoFactorSetupAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("GetTwoFactorSetup: User {UserId} not found", userId);
                    return new GeneralResult(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // Get or generate authenticator key
                var key = await _identityService.GetAuthenticatorKeyAsync(user.Data);
                if (string.IsNullOrWhiteSpace(key))
                {
                    await _identityService.ResetAuthenticatorKeyAsync(user.Data);
                    key = await _identityService.GetAuthenticatorKeyAsync(user.Data);
                }

                // Build QR Code URI
                var appName = "Lumora";
                var label = $"{appName}:{user.Data.Email}";
                var issuer = appName;

                var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(label)}?secret={key}&issuer={Uri.EscapeDataString(issuer)}&digits=6";

                _logger.LogInformation("2FA setup info generated for user {UserId}", userId);
                return new GeneralResult(true, messages.MsgTwoFASetupInfoGenerated, new { SharedSecretKey = key, QrCodeUri = qrCodeUri }, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating 2FA setup for user {UserId}", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" generating 2FA setup"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning("Refresh token is required.");
                    return new GeneralResult(false, messages.MsgRefreshTokenRequired, null, ErrorType.BadRequest);
                }

                var hashedToken = _tokenService.HashRefreshToken(refreshToken);
                var token = await _tokenRepository.GetValidRefreshTokenWithUserAsync(hashedToken, cancellationToken);

                if (token == null)
                {
                    _logger.LogWarning($"RefreshTokenAsync -  Refresh token : {token} is invalid or expired.");
                    return new GeneralResult(false, messages.MsgInvalidOrExpiredRefreshToken, null, ErrorType.BadRequest);
                }

                if (token.User == null || token.User.IsDeleted || !token.User.IsActive)
                {
                    _logger.LogWarning($"RefreshTokenAsync -  User : {token.User} is invalid or inactive.");
                    return new GeneralResult(false, messages.MsgUserInvalidOrInactive, null, ErrorType.BadRequest);
                }

                // Mark old token as used
                token.IsUsed = true;
                token.IsRevoked = true;

                // Issue new token
                var tokenResult = await IssueTokensAsync(token.User);
                if (tokenResult.IsSuccess == false) return tokenResult;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("RefreshTokenAsync - Refresh token succeeded for user {UserId}.", token.User.Id);
                return new GeneralResult(true, messages.MsgTokenRefreshed, tokenResult.Data, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshTokenAsync - Error during refresh token process.");
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" refresh token"), null, ErrorType.InternalServerError);
            }
        }


        #region Private methods

        /// <summary>
        /// Verifies the 2FA code.
        /// </summary>
        private async Task<GeneralResult> VerifyTwoFactorCodeAsync(User user, string twoFactorCode)
        {
            if (user == null)
            {
                return new GeneralResult(false, "User not provided for 2FA verification.");
            }

            if (twoFactorCode is null)
            {
                return new GeneralResult(false, "2FA code is missing.", null);
            }

            var isValid = await _identityService.VerifyTwoFactorTokenAsync(user, TwoFactorProvider.Authenticator, twoFactorCode.Trim());

            return isValid
                ? new GeneralResult(true, "2FA code is valid.")
                : new GeneralResult(false, "Invalid 2FA code.", null);
        }

        /// <summary>
        /// Issues tokens for the user.
        /// </summary>
        private async Task<GeneralResult> IssueTokensAsync(User user)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var tokenData = await _tokenService.GenerateTokenWithRefreshTokenAsync(user);
                if (string.IsNullOrWhiteSpace(tokenData?.AccessToken) || string.IsNullOrWhiteSpace(tokenData.RefreshToken))
                {
                    _logger.LogError("Token generation failed for user {UserId}", user.Id);
                    return new GeneralResult(false, "Failed to generate token.");
                }

                var hashedRefresh = _tokenService.HashRefreshToken(tokenData.RefreshToken);

                var refreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = hashedRefresh,
                    Expiration = now.AddDays(_jwtConfig.Value.RefreshTokenExpirationDays),
                    CreatedAt = now
                };

                await _tokenRepository.AddRefreshTokenAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync();

                // fetch user permissions and roles
                var roleNames = await _identityService.GetRolesAsync(user);
                var userRoles = new List<UserRoleDto>();

                foreach (var roleName in roleNames)
                {
                    var permissionResult = await _permissionService.GetPermissionsForRoleAsync(roleName);
                    var rolePermissions = permissionResult.Data ?? new List<string>();

                    userRoles.Add(new UserRoleDto
                    {
                        Name = roleName,
                        Permissions = rolePermissions
                            .Select(p => new UserPermissionsDto { PermissionName = p })
                            .ToList()
                    });
                }

                var result = new TokenDto
                {
                    AccessToken = tokenData.AccessToken,
                    RefreshToken = tokenData.RefreshToken,
                    AccessTokenExpiration = tokenData.AccessTokenExpiration,
                    TokenType = "Bearer",
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = userRoles
                };

                return new GeneralResult(true, "Token generated successfully.", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while issuing token for user {UserId}", user.Id);
                return new GeneralResult(false, "An error occurred while generating token.", ex.Message);
            }
        }
        #endregion
    }
}
