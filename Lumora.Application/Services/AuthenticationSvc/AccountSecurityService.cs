using Lumora.Application.Services.Authentication;

namespace Lumora.Application.Services.AuthenticationSvc
{
    public class AccountSecurityService(
        IIdentityRepository identityService,
        IUserService userService,
        AuthenticationMessage messages,
        IEmailConfirmation emailConfirmation,
        ILogger<AuthenticationService> logger
        ) : IAccountSecurityService
    {
        private readonly IEmailConfirmation _emailConfirmation = emailConfirmation;
        private readonly ILogger<AuthenticationService> _logger = logger;
        private readonly IIdentityRepository _identityService = identityService;

        /// <inheritdoc/>
        public async Task<GeneralResult> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserAsync(cancellationToken, null, null, dto.UserId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("ChangePassword : User not found or deleted or inactive. ID={UserId}", dto.UserId);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    _logger.LogWarning("ChangePassword: Passwords do not match for user {UserId}", dto.UserId);
                    return new GeneralResult(false, messages.MsgPasswordNotMatch, null, ErrorType.BadRequest);
                }

                var result = await _identityService.ChangePasswordAsync(user.Data, dto.CurrentPassword, dto.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("ChangePassword: {Code}", error);
                    }

                    return new GeneralResult(false, messages.MsgPasswordChangeFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("ChangePassword: Password changed successfully for user {UserId}", dto.UserId);
                return new GeneralResult(true, messages.MsgPasswordChanged, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while changing password for user {UserId}", dto.UserId);
                return new GeneralResult(false, messages.MsgPasswordChangeFailed, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken)
        {
            try
            {
                // 1: Find the user by email
                var user = await userService.FindUserAsync(cancellationToken, null, dto.PhoneNumber, null, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("ForgotPassword : User not found or deleted or inactive. PhoneNumber={PhoneNumber}", dto.PhoneNumber);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                // 2: Check if the user is confirmed
                if (!user.Data.PhoneNumberConfirmed)
                {
                    _logger.LogWarning("ForgotPassword: Phone number {PhoneNumber} is not confirmed", dto.PhoneNumber);
                    return new GeneralResult(false, messages.MsgPhoneNotConfirmed, null, ErrorType.BadRequest); // temp

                    // TODO: send confirmation sms

                    //var confirmResult = await _emailConfirmation.SendEmailConfirmation(user.Data);
                    //_logger.LogInformation("ForgotPassword: Email sent to user with Email {Email}", dto.Email);
                    //if (confirmResult.IsSuccess == false)
                    //{
                    //    return new GeneralResult(false, confirmResult.Message ?? messages.MsgEmailConfirmationFailed, confirmResult.Data, ErrorType.InternalServerError);
                    //}

                    //return new GeneralResult(true, confirmResult.Message ?? messages.MsgEmailConfirmationSent, confirmResult.Data, ErrorType.BadRequest);
                }
                else
                {
                    // 3: Generate a password reset token
                    var resetResult = await _emailConfirmation.SendEmailPasswordReset(user.Data, cancellationToken);
                    _logger.LogInformation("ForgotPassword: Email sent to user with PhoneNumber {PhoneNumber}", dto.PhoneNumber);
                    if (resetResult.IsSuccess == false)
                    {
                        return new GeneralResult(false, resetResult.Message ?? messages.MsgPasswordResetTokenGenerationFailed, resetResult.Data, ErrorType.InternalServerError);
                    }

                    return new GeneralResult(true, messages.MsgForgotPasswordEmailSent, null, ErrorType.Success);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending forgot password email for user with Email {Email}", dto.PhoneNumber);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" forgot password"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userService.FindUserAsync(cancellationToken, null, dto.PhoneNumber, null, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("ResetPassword : User not found or deleted or inactive. ID={UserId}", dto.PhoneNumber);
                    return new GeneralResult(false, user.Message ?? messages.MsgUserNotFound, null, user.ErrorType);
                }

                var result = await _identityService.ResetPasswordAsync(user.Data, dto.Token, dto.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("ResetPassword: Error for user {UserId} - {error}", user.Data.Id, error);
                    }

                    return new GeneralResult(false, messages.MsgPasswordResetFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("ResetPassword: Password reset successful for user {UserId}", user.Data.Id);
                return new GeneralResult(true, messages.MsgPasswordResetSuccessful, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetPassword: Exception occurred while resetting password for user with phone {PhoneNumber}", dto.PhoneNumber);
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage(" reset password"), null, ErrorType.InternalServerError);
            }
        }
    }
}
