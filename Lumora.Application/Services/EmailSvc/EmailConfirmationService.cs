namespace Lumora.Application.Services.Email
{
    public class EmailConfirmationService(
                IUserEmailRepository repository,
                IUserRepository userRepository,
                IUnitOfWork unitOfWork,
                ILogger<EmailConfirmationService> logger,
                IEmailSend emailSend,
                EmailSettings emailSettings) : IEmailConfirmation
    {
        private readonly IUserEmailRepository _repository = repository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<EmailConfirmationService> _logger = logger;
        private readonly IEmailSend _emailSend = emailSend;
        private readonly string _baseUrl = emailSettings.BaseUrl.TrimEnd('/');

        /// <inheritdoc/>
        public async Task<GeneralResult> SendEmailConfirmation(User user, CancellationToken cancellationToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1: find user
                if (user == null)
                    return new GeneralResult(false, "User not found.", null);

                // 2: generate token for user
                var token = await _repository.GenerateEmailConfirmationTokenAsync(user);

                // 3: save token in database
                await _repository.SetAuthenticationTokenAsync(user, "Default", "EmailVerificationToken", token);

                // 4: send email confirmation
                var confirmationLink = await GenerateConfirmationLinkAsync(user, token);
                if (confirmationLink != null)
                {
                    await _emailSend.SendEmailAsync(
                        user.Email!,
                        "Confirm Your Email Address",
                        $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

                    await transaction.CommitAsync(cancellationToken);
                    _logger.LogInformation("Confirmation link has been sent to user: {UserId}", user.Id);
                    return new GeneralResult
                    {
                        IsSuccess = true,
                        Message = "Confirmation link has been sent to your email.",
                        Data = null
                    };
                }
                else
                {
                    // 5: generate fallback link
                    _logger.LogError("Failed to generate confirmation link for user: {UserId}", user.Id);
                    await GenerateFallbackLinkAsync(user);
                    return new GeneralResult
                    {
                        IsSuccess = false,
                        Message = "Failed to send confirmation email. Please try again later or contact support.",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error sending confirmation email to user: {UserId}", user.Id);
                return new GeneralResult
                {
                    IsSuccess = false,
                    Message = "Failed to send confirmation email. Please try again later or contact support.",
                    Data = null
                };
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SendEmailPasswordReset(User user, CancellationToken cancellationToken)
        {
            // 1: find user
            if (user == null)
            {
                return new GeneralResult
                {
                    IsSuccess = false,
                    Message = "User not found.",
                    Data = null
                };
            }

            // 2: generate token for user
            var token = await _repository.GeneratePasswordResetTokenAsync(user);

            // 3: save token in database
            await _repository.SetAuthenticationTokenAsync(user, "Default", "PasswordResetToken", token);

            // 4: send email confirmation
            var confirmationLink = await GenerateConfirmationLinkAsync(user, token);
            if (confirmationLink != null)
            {
                await _emailSend.SendEmailAsync(
                    user.Email!,
                    "Reset Your Password",
                    $"Please reset your password by <a href='{confirmationLink}'>clicking here</a>.");

                _logger.LogInformation("Reset password link has been sent to user: {UserId}", user.Id);
                return new GeneralResult
                {
                    IsSuccess = true,
                    Message = "Password reset link has been sent to your email.",
                    Data = null
                };
            }
            else
            {
                // 5: generate fallback link
                _logger.LogError("Failed to generate reset password link for user: {UserId}", user.Id);
                return new GeneralResult
                {
                    IsSuccess = false,
                    Message = "Failed to send reset password email. Please try again later or contact support.",
                    Data = null
                };
            }
        }

        /// <inheritdoc/>
        public async Task<(bool Succeeded, string Message)> ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email address is required for resending verification link.");
                    return (false, "Email address is required.");
                }

                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("User not found for email: {Email}", email);
                    return (false, "User not found.");
                }

                if (await _repository.IsEmailConfirmedAsync(user))
                {
                    _logger.LogInformation("Email already confirmed for user: {Email}", email);
                    return (false, "Email is already confirmed.");
                }

                var storedToken = await _repository.GetAuthenticationTokenAsync(user, "Default", "EmailVerificationToken");
                var token = storedToken ?? await _repository.GenerateEmailConfirmationTokenAsync(user);
                if (storedToken == null)
                {
                    await _repository.SetAuthenticationTokenAsync(user, "Default", "EmailVerificationToken", token);
                    _logger.LogInformation("New confirmation token generated and stored for user: {Email}", email);
                }
                else
                {
                    _logger.LogInformation("Using existing confirmation token for user: {Email}", email);
                }

                var confirmationLink = await GenerateConfirmationLinkAsync(user, token);

                if (!string.IsNullOrEmpty(confirmationLink))
                {
                    await _emailSend.SendEmailAsync(
                        user.Email!,
                        "Confirmation Email",
                        $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
                }
                else
                {
                    _logger.LogError("Failed to generate confirmation link for user: {UserId}", user.Id);
                    await GenerateFallbackLinkAsync(user);
                    return (false, "Failed to send confirmation email. Please try again later or contact support.");
                }

                _logger.LogInformation("Confirmation email resent for user {UserId} to {Email}", user.Id, user.Email);
                return (true, "Confirmation email has been resent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification link for email: {Email}", email);
                return (false, "An error occurred while resending the verification email.");
            }
        }

        /// <inheritdoc/>   
        public async Task<(bool Succeeded, string Message, IEnumerable<string>? Errors)> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.UserId) || string.IsNullOrEmpty(dto.Token))
                {
                    _logger.LogWarning("Invalid confirmation request. Missing userId or token.");
                    return (false, "Invalid confirmation request.", null);
                }
                _logger.LogInformation("Confirming email for UserId: {UserId}", dto.UserId);

                var user = await _userRepository.GetByEmailAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found with Id: {UserId}", dto.UserId);
                    return (false, "User not found.", null);
                }

                var storedToken = await _repository.GetAuthenticationTokenAsync(user, "Default", "EmailVerificationToken");
                if (storedToken == null)
                {
                    _logger.LogWarning("No stored token found for UserId: {UserId}", dto.UserId);
                    return (false, "Invalid token, please request a new one.", null);
                }

                if (!string.Equals(storedToken, dto.Token, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Invalid token for UserId: {UserId}", dto.UserId);
                    return (false, "Invalid token.", null);
                }

                var result = await _repository.ConfirmEmailAsync(user, storedToken);
                if (result.Succeeded)
                {
                    var removeTokenResult = await _repository.RemoveAuthenticationTokenAsync(user, "Default", "EmailVerificationToken");
                    if (!removeTokenResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to remove confirmation token for UserId: {UserId}", dto.UserId);
                    }

                    _logger.LogInformation("Email confirmed successfully for UserId: {UserId}", dto.UserId);
                    return (true, "Email confirmed successfully!", null);
                }
                else
                {
                    var errorDetails = string.Join(", ", result.Errors);
                    _logger.LogError("Email confirmation failed for {Email}. Errors: {Errors}", user.Email, errorDetails);
                    return (false, "Email confirmation failed.", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for UserId: {UserId}", dto.UserId);
                return (false, "An error occurred while confirming email.", null);
            }
        }

        // Private methods

        // Generates a confirmation link for the user.
        private async Task<string> GenerateConfirmationLinkAsync(User user, string token)
        {
            try
            {
                var confirmationLink = $"{_baseUrl}/api/Email/ConfirmEmail?userId={user.Id}&token={token}";

                _logger.LogInformation("Generated confirmation link for user {UserId}: {ConfirmationLink}", user.Id, confirmationLink);
                return await Task.FromResult(confirmationLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating confirmation link for user {UserId}.", user.Id);
                throw;
            }
        }

        // Generates a fallback link for the user.
        private async Task<string> GenerateFallbackLinkAsync(User user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null.");
                }

                string fallbackToken = await _repository.GenerateUserTokenAsync(user, "Default", "FallbackConfirmation");
                string fallbackLink = $"{_baseUrl}/Account/ConfirmEmail?userId={user.Id}&token={fallbackToken}";

                _logger.LogInformation("Generated fallback link for user {UserId}: {FallbackLink}", user.Id, fallbackLink);

                var emailBody = $"""
                We encountered an issue generating the primary confirmation link. 
                Please confirm your account by <a href='{fallbackLink}'>clicking here</a>.
                If this issue persists, contact support for assistance. Thank you.
                """;

                await _emailSend.SendEmailAsync(user.Email!, "Alternative Email Confirmation Link", emailBody);
                return fallbackLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating fallback link for user {UserId}.", user?.Id);
                throw;
            }
        }
    }
}
