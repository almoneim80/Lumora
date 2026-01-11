namespace Lumora.Application.Interfaces.AuthenticationIntf
{
    public interface IAccountSecurityService
    {
        /// <summary>
        /// Changes the password for a user who is already logged in.
        /// </summary>
        Task<GeneralResult> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Initiates a password reset process for a user who has forgotten their password.
        /// Typically sends a reset link or code to the user's email.
        /// </summary>
        Task<GeneralResult> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Resets the user's password using a token provided by ForgotPasswordAsync.
        /// </summary>
        Task<GeneralResult> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken);
    }
}
