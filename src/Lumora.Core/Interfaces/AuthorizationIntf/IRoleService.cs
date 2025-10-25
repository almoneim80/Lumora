namespace Lumora.Interfaces.Authorization
{
    public interface IRoleService
    {
        /// <summary>
        /// Bring in all the roles.
        /// </summary>
        Task<GeneralResult<List<string>>> GetAllRolesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Check for a specific role.
        /// </summary>
        Task<GeneralResult<bool>> RoleExistsAsync(string roleName);

        /// <summary>
        /// Get all users in a specific role.
        /// </summary>
        Task<GeneralResult<List<User>>> GetUsersInRoleAsync(string roleName);

        /// <summary>
        /// Make sure the user is assigned to a specific role.
        /// </summary>
        Task<GeneralResult<bool>> IsUserInRoleAsync(string userId, string roleName, CancellationToken cancellationToken);

        /// <summary>
        /// Ensure virtual roles exist.
        /// </summary>
        Task<GeneralResult> EnsureSeedRolesAsync();

        /// <summary>
        /// Add a new role. 
        /// </summary>
        Task<GeneralResult> AddRoleAsync(string roleName);

        /// <summary>
        ///  Assign a role to the user.
        /// </summary>
        Task<GeneralResult> AssignRoleAsync(string userId, string role);

        /// <summary>
        ///  Delete roles.
        /// </summary>
        Task<GeneralResult> DeleteRoleAsync(string roleName, CancellationToken cancellationToken);

        /// <summary>
        /// Update Role Name.
        /// </summary>
        Task<GeneralResult> UpdateRoleNameAsync(string oldRoleName, string newRoleName);

        /// <summary>
        /// Removing a role from a user.
        /// </summary>
        Task<GeneralResult> RemoveRoleAsync(string userId, string role);

        /// <summary>
        /// Get all roles for a specific user.
        /// </summary>
        Task<GeneralResult<List<string>>> GetUserRolesAsync(string userId);
    }
}
