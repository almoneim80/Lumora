using Lumora.Application.DTOs;
using Lumora.Application.Interfaces.AuthenticationIntf;
using Lumora.Infrastructure.Exceptions;
using Lumora.Interfaces.CustomInf.UserIntf;

namespace Lumora.Infrastructure.Repositories
{
    public class IdentityRepository(
        UserManager<User> userManager,
        IUserService userService,
        SignInManager<User> signInManager,
        ILogger<IdentityRepository> logger) : IIdentityRepository
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly ILogger<IdentityRepository> _logger = logger;
        private readonly IUserService _userService = userService;

        /// <inheritdoc/>
        public async Task<OperationResult> CheckPasswordAsync(User user, string password)
        {
            // 1. استخدام CheckPasswordSignInAsync لأنه يقوم بكل العمل الشاق:
            // - التحقق من صحة كلمة المرور (Hashing).
            // - التحقق مما إذا كان الحساب مقفلاً (Lockout).
            // - زيادة عداد المحاولات الخاطئة (Access Failed Count) إذا فشل الدخول.

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            // 2. تحويل نتيجة Microsoft Identity إلى الـ OperationResult الخاص بنا (Abstraction)
            if (result.Succeeded)
            {
                return OperationResult.Success();
            }

            if (result.IsLockedOut)
            {
                return OperationResult.FailedLockedOut();
            }

            if (result.IsNotAllowed)
            {
                // تستخدم عادة إذا كان الإيميل غير مؤكد أو الحساب غير مفعل
                return OperationResult.Failed("Account not allowed to sign in.");
            }

            // الافتراضي هو فشل بسبب بيانات خاطئة
            return OperationResult.Failed("Invalid password.");
        }

        /// <inheritdoc/>
        public async Task<User> FindOnRegister(string phoneNumber, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    throw new ArgumentException("PhoneNumber cannot be null or empty.", nameof(phoneNumber));
                }

                var user = await _userService.FindUserAsync(cancellationToken, null, phoneNumber, null, true);
                if (user.Data == null)
                {
                    user.Data = new User
                    {
                        FullName = phoneNumber,
                        PhoneNumber = phoneNumber,
                        CreatedAt = DateTime.UtcNow,
                    };

                    var result = await _userManager.CreateAsync(user.Data);

                    if (!result.Succeeded)
                    {
                        _logger.LogError("Failed to create user with PhoneNumber {PhoneNumber}. Errors: {Errors}", phoneNumber, result.Errors);
                        throw new IdentityException(result.Errors);
                    }

                    _logger.LogInformation("User with PhoneNumber {PhoneNumber} created successfully.", phoneNumber);
                }
                else
                {
                    _logger.LogInformation("User with PhoneNumber {PhoneNumber} found.", phoneNumber);
                }

                return user.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during FindOnRegister for phoneNumber {phoneNumber}.", phoneNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ClaimsPrincipal> CreateUserClaimsPrincipal(User user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null.");
                }

                var claims = await CreateUserClaims(user);

                var identity = new ClaimsIdentity(claims);
                _logger.LogInformation("ClaimsPrincipal created successfully for user {UserId}.", user.Id);

                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ClaimsPrincipal for user {UserId}.", user?.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Claim>> CreateUserClaims(User user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null.");
                }

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("LoginProvider", "Google"),
            };

                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                _logger.LogInformation("Claims created successfully for user {UserId}.", user.Id);
                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claims for user {UserId}.", user?.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<OperationResult> ResetAuthenticatorKeyAsync(User user)
        {
            try
            {
                var result = await _userManager.ResetAuthenticatorKeyAsync(user);
                if (result.Succeeded)
                {
                    return OperationResult.Success();
                }

                return OperationResult.Failed(result.Errors.Select(e => e.Description).ToArray());
            }
            catch (Exception)
            {
                return OperationResult.Failed("An error occurred while resetting the 2FA key.");
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetAuthenticatorKeyAsync(User user)
        {
            // Retrieve the existing key in the database (AspNetUserTokens)
            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            if (key == null)
            {
                // Handle the case where the key is null, e.g., return an error message or a default value
                return "No key found.";
            }

            return key;
        }

        public async Task<bool> VerifyTwoFactorTokenAsync(User user, TwoFactorProvider provider, string token)
        {
            string providerName = provider switch
            {
                TwoFactorProvider.Authenticator => TokenOptions.DefaultAuthenticatorProvider,
                TwoFactorProvider.Email => TokenOptions.DefaultEmailProvider,
                TwoFactorProvider.Sms => TokenOptions.DefaultPhoneProvider,
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            };

            return await _userManager.VerifyTwoFactorTokenAsync(user, providerName, token);
        }

        public async Task<List<string>> GetRolesAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<OperationResult> CreateAsync(User newUser, string password)
        {
            var result = await _userManager.CreateAsync(newUser, password);
            return ToOperationResult(result);
        }

        public async Task<OperationResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return ToOperationResult(result);
        }

        public async Task<OperationResult> ConfirmEmailAsync(User user, string confirmationToken)
        {
            var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);
            return ToOperationResult(result);
        }

        public async Task<OperationResult> VerifyChangePhoneNumberTokenAsync(User user, string verificationCode, string phoneNumber)
        {
            var result = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, verificationCode);
            return ToOperationResult(result);
        }

        public async Task<OperationResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return ToOperationResult(result);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User signed out successfully.");
        }

        public async Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return await _userManager.GetTwoFactorEnabledAsync(user);
        }

        public async Task<bool> SetTwoFactorEnabledAsync(User user, bool active)
        {
            var result = await _userManager.SetTwoFactorEnabledAsync(user, active);
            return result.Succeeded;
        }

        public async Task<OperationResult> UpdateUserAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            return ToOperationResult(result);
        }

        // --- Private Helper Methods لتجنب التكرار ---

        private OperationResult ToOperationResult(IdentityResult result)
        {
            if (result.Succeeded)
            {
                return OperationResult.Success();
            }

            var errors = result.Errors.Select(e => e.Description).ToArray();
            _logger.LogWarning("Identity Operation Failed: {Errors}", string.Join(", ", errors));

            return OperationResult.Failed(errors);
        }
    }
}
