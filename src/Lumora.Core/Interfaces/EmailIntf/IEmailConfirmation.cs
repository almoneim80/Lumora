using Lumora.DTOs.Authentication;
using Lumora.DTOs.TrainingProgram;

namespace Lumora.Interfaces.Email
{
    public interface IEmailConfirmation
    {
        /// <summary>
        /// Sends an email confirmation link to the user.
        /// </summary>
        Task<GeneralResult> SendEmailConfirmation(User user);

        /// <summary>
        /// Sends an email password reset link to the user.
        /// </summary>
        Task<GeneralResult> SendEmailPasswordReset(User user);

        /// <summary>
        /// Confirms the user's email address using a verification token.
        /// </summary>
        Task<(bool Succeeded, string Message, IEnumerable<IdentityError>? Errors)> ConfirmEmailAsync(ConfirmEmailDto dto);

        /// <summary>
        /// Resends an email confirmation link to the user.
        /// </summary>
        Task<(bool Succeeded, string Message)> ResendEmailConfirmationAsync(string email);
    }
}
