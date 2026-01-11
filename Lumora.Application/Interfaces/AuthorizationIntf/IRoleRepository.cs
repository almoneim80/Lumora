namespace Lumora.Application.Interfaces.AuthorizationIntf
{
    public interface IRoleRepository
    {
        Task<bool> RoleExistsAsync(string roleName);
        Task<OperationResult> CreateRoleAsync(string roleName);
        Task<AppRole?> FindRoleByNameAsync(string roleName);
        Task<OperationResult> DeleteRoleAsync(AppRole roleName);
        Task<OperationResult> UpdateRoleAsync(AppRole roleName);

        // User-Role Operations
        Task<OperationResult> AddToRoleAsync(User user, string roleName);
        Task<OperationResult> RemoveFromRoleAsync(User user, string roleName);
        Task<IList<User>> GetUsersInRoleAsync(string roleName);
        Task<IList<string>> GetUserRolesAsync(User user);
        Task<bool> IsInRoleAsync(User user, string roleName);
        Task<List<string>> GetAllRoleNamesAsync(CancellationToken cancellationToken);
    }
}
