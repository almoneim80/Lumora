using Lumora.DTOs.Authentication;
namespace Lumora.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Registers a new user with the provided registration data.
        /// </summary>
        Task<GeneralResult> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Authenticates a user with the provided login credentials.
        /// </summary>
        Task<GeneralResult> LoginAsync(LoginDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Authenticates a user with the provided login credentials.
        /// </summary>
        Task<GeneralResult> LoginWith2FACodeAsync(Login2FADto dto, CancellationToken cancellationToken);

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

        /// <summary>
        /// Logs out the current user and revokes any active tokens if necessary.
        /// </summary>
        Task<GeneralResult> LogoutAsync(string userId, string refreshToken, CancellationToken cancellationToken);

        /// <summary>
        /// Verifies and activates two-factor authentication (2FA) for the user (optional).
        /// </summary>
        Task<GeneralResult> EnableTwoFactorAuthAsync(Enable2FADto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Disables two-factor authentication for the user (optional).
        /// </summary>
        Task<GeneralResult> DisableTwoFactorAuthAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// generate 2fa secret key for user.
        /// </summary>
        Task<GeneralResult> GetTwoFactorSetupAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Refreshes the access token using the provided refresh token.
        /// </summary>
        Task<GeneralResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

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

        /// <summary>
        /// Changes the phone number for a user with the provided user ID and new phone number.
        /// </summary>
        Task<GeneralResult> ChangePhoneNumberAsync(string userId, string newPhoneNumber, CancellationToken cancellationToken);

        /// <summary>
        /// Completes user data (such as adding personal data or any missing information).
        /// </summary>
        Task<GeneralResult> CompleteProfileAsync(string userId, CompleteUserDataDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates user information.
        /// </summary>
        Task<GeneralResult> UpdateProfileAsync(string userId, UserUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the profile information for a user with the provided user ID.
        /// </summary>
        Task<GeneralResult<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken);
    }
}
