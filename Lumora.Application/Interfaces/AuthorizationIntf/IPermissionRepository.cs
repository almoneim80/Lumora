namespace Lumora.Application.Interfaces.AuthorizationIntf
{
    public interface IPermissionRepository
    {
        // Role-Claim Operations
        Task<AppRole?> FindRoleByNameAsync(string roleName);
        Task<IList<Claim>> GetRoleClaimsAsync(AppRole role);
        Task<OperationResult> AddClaimToRoleAsync(AppRole role, Claim claim);
        Task<OperationResult> RemoveClaimFromRoleAsync(AppRole role, Claim claim);

        // User-Claim Operations
        Task<IList<Claim>> GetUserClaimsAsync(User user);
        Task<OperationResult> RemoveClaimFromUserAsync(User user, Claim claim);

        // User-Role Operations (Needed for Permission Aggregation)
        Task<IList<string>> GetUserRolesAsync(User user);

        Task<bool> RoleHasPermissionAsync(AppRole role, string permission);
        Task<OperationResult> AddPermissionToRoleAsync(AppRole role, string permission);
        Task<List<string>> GetRoleClaimsValuesByTypeAsync(string roleName, string claimType);
    }
}
