namespace Lumora.Application.Interfaces.Authentication
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
    }
}
