namespace Lumora.Application.Interfaces.AuthenticationIntf
{
    public interface IUserActivationService
    {
        /// <summary>
        /// Confirms the email address of a user using the provided confirmation token.
        /// </summary>
        Task<GeneralResult> ConfirmEmailAsync(string userId, string confirmationToken, CancellationToken cancellationToken);

        /// <summary>
        /// Confirms the phone number of a user using the provided verification code.
        /// </summary>
        Task<GeneralResult> ConfirmPhoneAsync(string userId, string verificationCode, CancellationToken cancellationToken);

        /// <summary>
        /// Resends the confirmation email for a user with the provided email address.
        /// </summary>
        Task<GeneralResult> ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken);

        /// <summary>
        /// Resends the confirmation SMS for a user with the provided phone number.
        /// </summary>
        Task<GeneralResult> ResendConfirmationSmsAsync(string phoneNumber, CancellationToken cancellationToken);

        /// <summary>
        /// Deactivates a user with the provided user ID.
        /// </summary>
        Task<GeneralResult> DeactivateUserAsync(string userId, CancellationToken cancellationToken, string? reason = null);

        /// <summary>
        /// Activates a user with the provided user ID.
        /// </summary>
        Task<GeneralResult> ActivateUserAsync(string userId, CancellationToken cancellationToken);
    }
}
