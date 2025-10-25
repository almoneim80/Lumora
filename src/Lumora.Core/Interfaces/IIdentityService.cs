namespace Lumora.Interfaces
{
    public interface IIdentityService
    {
        /// <summary>
        /// Create a ClaimsPrincipal for the user.
        /// </summary>
        Task<ClaimsPrincipal> CreateUserClaimsPrincipal(User user);

        /// <summary>
        /// Create a Claims list for the user.
        /// </summary>
        Task<List<Claim>> CreateUserClaims(User user);
    }
}
