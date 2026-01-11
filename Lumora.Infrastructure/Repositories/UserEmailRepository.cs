using Lumora.Application.DTOs;
using Lumora.Application.Interfaces.EmailIntf;

namespace Lumora.Infrastructure.Repositories
{
    public class UserEmailRepository : IUserEmailRepository
    {
        private readonly UserManager<User> _userManager;
        public UserEmailRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> FindByIdAsync(string userId) => await _userManager.FindByIdAsync(userId);
        public async Task<User?> FindByEmailAsync(string email) => await _userManager.FindByEmailAsync(email);
        public async Task<bool> IsEmailConfirmedAsync(User user) => await _userManager.IsEmailConfirmedAsync(user);
        public async Task<string> GenerateEmailConfirmationTokenAsync(User user) => await _userManager.GenerateEmailConfirmationTokenAsync(user);
        public async Task<string> GeneratePasswordResetTokenAsync(User user) => await _userManager.GeneratePasswordResetTokenAsync(user);
        public async Task<string> GenerateUserTokenAsync(User user, string provider, string purpose) => await _userManager.GenerateUserTokenAsync(user, provider, purpose);
        public async Task<string?> GetAuthenticationTokenAsync(User user, string provider, string name) => await _userManager.GetAuthenticationTokenAsync(user, provider, name);
        public async Task<OperationResult> SetAuthenticationTokenAsync(User user, string provider, string name, string value)
        {
            var identityResult = await _userManager.SetAuthenticationTokenAsync(user, provider, name, value);

            return identityResult.Succeeded
                ? OperationResult.Success()
                : OperationResult.Failed(identityResult.Errors.Select(e => e.Description).ToArray());
        }
        public async Task<OperationResult> RemoveAuthenticationTokenAsync(User user, string provider, string name)
        {
            var identityResult = await _userManager.RemoveAuthenticationTokenAsync(user, provider, name);

            return identityResult.Succeeded
                ? OperationResult.Success()
                : OperationResult.Failed(identityResult.Errors.Select(e => e.Description).ToArray());
        }
        public async Task<OperationResult> ConfirmEmailAsync(User user, string token)
        {
            var identityResult = await _userManager.ConfirmEmailAsync(user, token);

            return identityResult.Succeeded
                ? OperationResult.Success()
                : OperationResult.Failed(identityResult.Errors.Select(e => e.Description).ToArray());
        }
    }
}
