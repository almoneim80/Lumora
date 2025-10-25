namespace Lumora.Interfaces.Authorization
{
    public interface IPermissionService
    {
        /// <summary>
        /// adding premission to role by role name.
        /// </summary>
        Task<GeneralResult> AddPermissionToRoleAsync(string roleName, string permission);

        /// <summary>
        /// adding list of premission to role by role name.
        /// </summary>
        Task<GeneralResult> AddPermissionsToRoleAsync(string roleName, List<string> permissions);

        /// <summary>
        /// remove permission from role by role name.
        /// </summary>
        Task<GeneralResult> RemovePermissionFromRoleAsync(string roleName, string permission);

        /// <summary>
        /// get all permissions for a specific role.
        /// </summary>
        Task<GeneralResult<List<string>>> GetPermissionsForRoleAsync(string roleName);

        /// <summary>
        /// get all permissions for a specific user.
        /// </summary>
        Task<GeneralResult<List<string>>> GetPermissionsForUserAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// check if user has permission.
        /// </summary>
        Task<GeneralResult<bool>> UserHasPermissionAsync(string userId, CancellationToken cancellationToken, string permission);

        /// <summary>
        /// remove permission from user by user id.
        /// </summary>
        Task<GeneralResult> RemovePermissionFromUserAsync(string userId, CancellationToken cancellationToken, string permission);

        ///// <summary>
        ///// check if user can access enrollment.
        ///// </summary>
        //Task<bool> CanAccessEnrollmentAsync(string userId, ProgramEnrollment enrollment, CancellationToken cancellationToken);
    }
}
