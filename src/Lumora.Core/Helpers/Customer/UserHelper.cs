namespace Lumora.Helpers.Customer
{
    public static class UserHelper
    {
        /// <summary>
        /// Get current user id.
        /// </summary>
        public static async Task<string?> GetCurrentUserIdAsync(UserManager<User> userManager, ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                return null;
            }

            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out Guid guid))
            {
                return userId;
            }

            var user = await GetCurrentUserAsync(userManager, claimsPrincipal);

            if (user != null)
            {
                return user.Id;
            }

            return null;
        }

        /// <summary>
        /// Get current user.
        /// </summary>
        public static async Task<User> GetCurrentUserOrThrowAsync(UserManager<User> userManager, ClaimsPrincipal? claimsPrincipal)
        {
            var user = await GetCurrentUserAsync(userManager, claimsPrincipal);

            return user == null ? throw new UnauthorizedAccessException() : user;
        }

        /// <summary>
        /// Get current user.
        /// </summary>
        public static async Task<User?> GetCurrentUserAsync(UserManager<User> userManager, ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal == null || claimsPrincipal.Identity == null)
            {
                return null;
            }

            if (claimsPrincipal.Identity.IsAuthenticated && !claimsPrincipal.Identity!.AuthenticationType!.Contains("Federation"))
            {
                var user = await userManager.Users
                    .Where(u => u.UserName == claimsPrincipal.Identity.Name && !u.IsDeleted)
                    .FirstOrDefaultAsync();
                return user;
            }

            var userEmail = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type.Contains("emailaddress"))?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                return await userManager.Users
                    .Where(u => u.Email == userEmail && !u.IsDeleted)
                    .FirstOrDefaultAsync();
            }

            var userIdentifier = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type.Contains("nameidentifier"))?.Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(userIdentifier))
            {
                return await userManager.Users
                    .Where(u => u.Id == userIdentifier && !u.IsDeleted)
                    .FirstOrDefaultAsync();
            }

            return null;
        }
    }
}
