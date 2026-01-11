namespace Lumora.Application.Interfaces.AuthenticationIntf
{
    public interface IIdentityRepository
    {
        /// <summary>
        /// Create a ClaimsPrincipal for the user.
        /// </summary>
        Task<ClaimsPrincipal> CreateUserClaimsPrincipal(User user);

        Task<List<string>> GetRolesAsync(User user);

        /// <summary>
        /// register new user.
        /// </summary>
        Task<OperationResult> CreateAsync(User newUser, string password);

        /// <summary>
        /// change user password.
        /// </summary>
        Task<OperationResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);

        Task<OperationResult> ConfirmEmailAsync(User user, string confirmationToken);
        Task<OperationResult> VerifyChangePhoneNumberTokenAsync(User user, string verificationCode, string phoneNumber);

        /// <summary>
        /// allow user to reset his password.
        /// </summary>
        Task<OperationResult> ResetPasswordAsync(User user, string token, string newPassword);
        Task<User> FindOnRegister(string phoneNumber, CancellationToken cancellationToken);


        Task<string> GetAuthenticatorKeyAsync(User user);
        Task<bool> VerifyTwoFactorTokenAsync(User user, TwoFactorProvider provider, string token);
        Task<OperationResult> ResetAuthenticatorKeyAsync(User user);

        /// <summary>
        /// Create a Claims list for the user.
        /// </summary>
        Task<List<Claim>> CreateUserClaims(User user);

        /// <summary>
        /// check if password is correct.
        /// </summary>
        Task<OperationResult> CheckPasswordAsync(User user, string password);

        /// <summary>
        /// signout.
        /// </summary>
        Task SignOutAsync();

        /// <summary>
        /// enable user 2FA.
        /// </summary>
        Task<bool> GetTwoFactorEnabledAsync(User user);

        /// <summary>
        /// enable / disable 2FA.
        /// </summary>
        Task<bool> SetTwoFactorEnabledAsync(User user, bool active);

        /// <summary>
        /// Update user data.
        /// </summary>
        Task<OperationResult> UpdateUserAsync(User user);
    }
}
