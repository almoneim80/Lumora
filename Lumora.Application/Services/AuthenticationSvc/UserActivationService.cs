using Lumora.Application.Services.Authentication;
namespace Lumora.Application.Services.AuthenticationSvc
{
    public class UserActivationService(
        IIdentityRepository identityService,
        IUserService userService,
        AuthenticationMessage messages,
        IEmailConfirmation emailConfirmation,
        ILogger<AuthenticationService> logger) : IUserActivationService
    {
        private readonly IEmailConfirmation _emailConfirmation = emailConfirmation;
        private readonly ILogger<AuthenticationService> _logger = logger;
        private readonly IIdentityRepository _identityService = identityService;

        /// <inheritdoc/>
        public async Task<GeneralResult> ConfirmEmailAsync(string userId, string confirmationToken, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(confirmationToken))
                {
                    return new GeneralResult(false, messages.MsgUserIdAndConfirmationTokenRequired, null, ErrorType.BadRequest);
                }

                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("CompleteProfileAsync: User not found or deleted or inactive. userId={userId}", userId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                if (user.Data.EmailConfirmed)
                {
                    _logger.LogWarning("ResendConfirmationEmailAsync: Email is already confirmed. ID={UserId}", user.Data.Id);
                    return new GeneralResult(true, messages.MsgEmailAlreadyConfirmed, null, ErrorType.BadRequest);
                }

                var result = await _identityService.ConfirmEmailAsync(user.Data, confirmationToken);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Email confirmed for user {UserId}.", user.Data.Id);
                    return new GeneralResult(true, messages.MsgEmailConfirmed, null, ErrorType.Success);
                }

                _logger.LogWarning($"Failed to confirm email for user {userId}, errors: {string.Join(",", result.Errors)}.", user.Data.Id);
                return new GeneralResult(false, messages.MsgEmailConfirmationFailed, null, ErrorType.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user {UserId}.", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" confirming email"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ConfirmPhoneAsync(string userId, string verificationCode, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(verificationCode))
                {
                    _logger.LogWarning("UserId and verification code are required.");
                    return new GeneralResult(false, messages.MsgUserIdAndVerificationCodeRequired, null, ErrorType.BadRequest);
                }

                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("CompleteProfileAsync: User not found or deleted or inactive. ID={UserId}", userId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                // Verify verification code from authenticator app.
                var isValid = await _identityService.VerifyChangePhoneNumberTokenAsync(user.Data, verificationCode, user.Data.PhoneNumber!);
                if (!isValid.Succeeded)
                {
                    _logger.LogWarning("Invalid phone confirmation code for user {UserId}.", userId);
                    return new GeneralResult(false, messages.MsgInvalidOrExpiredCode, null, ErrorType.BadRequest);
                }

                user.Data.PhoneNumberConfirmed = true;
                var result = await _identityService.UpdateUserAsync(user.Data);
                if (result.Succeeded == false)
                {
                    _logger.LogWarning($"Failed to confirm phone number for user ID:{userId}." + $" Errors: {result.Errors}.");
                    return new GeneralResult(false, messages.MsgPhoneNumberConfirmationFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("Phone number confirmed for user {UserId}.", userId);
                return new GeneralResult(true, messages.MsgPhoneNumberConfirmed, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming phone number for user {UserId}.", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" confirming phone number."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("ResendConfirmationEmailAsync: Email is required.");
                    return new GeneralResult(false, messages.MsgEmailRequired, null, ErrorType.BadRequest);
                }

                var user = await userService.FindUserAsync(cancellationToken, email, null, null, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("CompleteProfileAsync: User not found or deleted or inactive. email={email}", email);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                if (user.Data.EmailConfirmed)
                {
                    _logger.LogWarning("ResendConfirmationEmailAsync: Email is already confirmed. ID={UserId}", user.Data.Id);
                    return new GeneralResult(true, messages.MsgEmailAlreadyConfirmed, null, ErrorType.BadRequest);
                }

                var result = await _emailConfirmation.SendEmailConfirmation(user.Data, cancellationToken);
                if (result.IsSuccess == false)
                {
                    _logger.LogWarning("ResendConfirmationEmailAsync: Failed to send confirmation email to {Email}.", email);
                    return new GeneralResult(false, result.Message ?? messages.MsgEmailConfirmationFailed, result.Data, ErrorType.InternalServerError);
                }

                _logger.LogInformation("ResendConfirmationEmailAsync: Confirmation email sent to {Email}.", email);
                return new GeneralResult(true, messages.MsgEmailConfirmationSent, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending confirmation email to {Email}", email);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage("resending confirmation email"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ResendConfirmationSmsAsync(string phoneNumber, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    _logger.LogWarning("ResendConfirmationSmsAsync: Phone number is required.");
                    return new GeneralResult(false, messages.MsgPhoneNumberRequired, null, ErrorType.BadRequest);
                }

                var user = await userService.FindUserWithoutPhoneNumberConfirmedAsync(cancellationToken, null, phoneNumber);
                if (user.Data == null)
                {
                    _logger.LogWarning("CompleteProfileAsync: User not found or deleted or inactive. phoneNumber={phoneNumber}", phoneNumber);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                if (user.Data.PhoneNumberConfirmed)
                {
                    _logger.LogWarning("ResendConfirmationSmsAsync: Phone number is already confirmed. phoneNumber={phoneNumber}", phoneNumber);
                    return new GeneralResult(true, messages.MsgPhoneAlreadyConfirmed, null, ErrorType.BadRequest);
                }

                // TODO: Send SMS confirmation

                return new GeneralResult(true, messages.MsgPhoneConfirmationSent, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending SMS confirmation to {PhoneNumber}", phoneNumber);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage("resending SMS confirmation"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeactivateUserAsync(string userId, CancellationToken cancellationToken, string? reason = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ActivateUserAsync: User ID is required.");
                    return new GeneralResult(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await userService.GetUserByIdWithoutActiveValidation(userId);
                if (user.Data == null)
                {
                    _logger.LogWarning("DeactivateUserAsync: User not found. ID={UserId}", userId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (!user.Data.IsActive)
                {
                    _logger.LogInformation("DeactivateUserAsync: User already inactive. ID={UserId}", userId);
                    return new GeneralResult(true, messages.MsgAccountAlreadyDeactivated, null, ErrorType.BadRequest);
                }

                user.Data.IsActive = false;
                user.Data.DeActiveReason = reason;
                user.Data.UpdatedAt = DateTimeOffset.UtcNow;
                await _identityService.UpdateUserAsync(user.Data);

                // TODO: Send deactivation email with reason.
                await _identityService.SignOutAsync();
                _logger.LogInformation("DeactivateUserAsync: User deactivated. ID={UserId}", userId);
                return new GeneralResult(true, messages.MsgAccountDeactivated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}.", userId);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage("deactivate account."), ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ActivateUserAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ActivateUserAsync: User ID is required.");
                    return new GeneralResult(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await userService.GetUserByIdWithoutActiveValidation(userId);
                if (user.Data == null)
                {
                    _logger.LogWarning("ActivateUserAsync: User not found. ID={UserId}", userId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (user.Data.IsActive)
                {
                    _logger.LogWarning("ActivateUserAsync: User already active. ID={UserId}", userId);
                    return new GeneralResult(true, messages.MsgAccountAlreadyActivated, null, ErrorType.BadRequest);
                }

                user.Data.IsActive = true;
                user.Data.UpdatedAt = DateTimeOffset.UtcNow;
                var result = await _identityService.UpdateUserAsync(user.Data);

                if (result.Succeeded == false)
                {
                    _logger.LogWarning("ActivateUserAsync: Failed to activate user. ID={UserId}", userId);
                    return new GeneralResult(false, messages.MsgAccountActivationFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("User {UserId} activated successfully.", userId);
                // TODO: Send activation email

                return new GeneralResult(true, messages.MsgAccountActivated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}.", userId);
                return new GeneralResult(
                    false, messages.GetUnexpectedErrorMessage(" activating your account. please try again or contact support."), null, ErrorType.InternalServerError);
            }
        }
    }
}
