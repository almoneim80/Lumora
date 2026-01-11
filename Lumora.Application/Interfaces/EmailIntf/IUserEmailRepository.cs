namespace Lumora.Application.Interfaces.EmailIntf
{
    public interface IUserEmailRepository
    {
        Task<bool> IsEmailConfirmedAsync(User user);
        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<string> GenerateUserTokenAsync(User user, string provider, string purpose);
        Task<OperationResult> SetAuthenticationTokenAsync(User user, string provider, string name, string value);
        Task<string?> GetAuthenticationTokenAsync(User user, string provider, string name);
        Task<OperationResult> RemoveAuthenticationTokenAsync(User user, string provider, string name);
        Task<OperationResult> ConfirmEmailAsync(User user, string token);
    }
}
