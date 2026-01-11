namespace Lumora.Application.Interfaces.Email
{
    public interface IEmailConfirmation
    {
        /// <summary>
        /// Sends an email confirmation link to the user.
        /// </summary>
        Task<GeneralResult> SendEmailConfirmation(User user, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an email password reset link to the user.
        /// </summary>
        Task<GeneralResult> SendEmailPasswordReset(User user, CancellationToken cancellationToken);

        /// <summary>
        /// Confirms the user's email address using a verification token.
        /// </summary>
        Task<(bool Succeeded, string Message, IEnumerable<string>? Errors)> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Resends an email confirmation link to the user.
        /// </summary>
        Task<(bool Succeeded, string Message)> ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken);
    }
}
