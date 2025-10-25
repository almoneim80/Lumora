using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.Customer;
using Lumora.Services.Core.Messages;

namespace Lumora.Services.Authorization
{
    public class PermissionService(RoleManager<IdentityRole> roleManager, IUserService userService, UserManager<User> userManager, ILogger<PermissionService> logger, PermissionMessage messages) : IPermissionService
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<PermissionService> _logger = logger;

        /// <inheritdoc/>
        public async Task<GeneralResult> AddPermissionToRoleAsync(string roleName, string permission)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permission))
                {
                    _logger.LogError("PermissionService - AddPermissionToRoleAsync : Permission cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgPermissionCannotBeEmpty, null, ErrorType.BadRequest);
                }

                if (!Permissions.All.Contains(permission))
                {
                    _logger.LogWarning("Tried to add undefined permission: {Permission}", permission);
                    return new GeneralResult(false, messages.MsgPermissionNotDefined, null, ErrorType.NotFound);
                }

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("PermissionService - AddPermissionToRoleAsync : AddPermissionToRole: Role '{RoleName}' not found.", roleName);
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                var existingClaims = await _roleManager.GetClaimsAsync(role);
                if (existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    _logger.LogInformation("PermissionService - AddPermissionToRoleAsync : AddPermissionToRole: Permission '{Permission}' already exists for role '{RoleName}'.", permission, roleName);
                    return new GeneralResult(false, messages.MsgPermissionAlreadyExistsForRole, null, ErrorType.BadRequest);
                }

                var claim = new Claim("Permission", permission);
                var result = await _roleManager.AddClaimAsync(role, claim);

                if (!result.Succeeded)
                {
                    _logger.LogError("PermissionService - AddPermissionToRoleAsync : AddPermissionToRole: Failed to add permission '{Permission}' to role '{RoleName}'. Errors: {Errors}", permission, roleName, result.Errors);
                    return new GeneralResult(false, messages.MsgAddPermissionFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("PermissionService - AddPermissionToRoleAsync : AddPermissionToRole: Successfully added permission '{Permission}' to role '{RoleName}'.", permission, roleName);
                return new GeneralResult(true, messages.MsgAddPermissionSucceeded, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService - AddPermissionToRoleAsync : AddPermissionToRole: Unexpected error while adding permission '{Permission}' to role '{RoleName}'.", permission, roleName);
                return new GeneralResult(false, messages.MsgUnexpectedAddPermissionError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> AddPermissionsToRoleAsync(string roleName, List<string> permissions)
        {
            try
            {
                if (permissions == null || !permissions.Any())
                {
                    _logger.LogError("PermissionService - AddPermissionsToRoleAsync : Permissions list cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgPermissionsListCannotBeEmpty, null, ErrorType.BadRequest);
                }

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("PermissionService - AddPermissionsToRoleAsync : AddPermissionsToRole: Role '{RoleName}' not found.", roleName);
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                var existingClaims = await _roleManager.GetClaimsAsync(role);
                var distinctPermissions = permissions.Distinct();
                var failedPermissions = new List<string>();

                foreach (var permission in distinctPermissions)
                {
                    if (string.IsNullOrWhiteSpace(permission))
                    {
                        _logger.LogWarning("PermissionService - AddPermissionsToRoleAsync : AddPermissionsToRole: Encountered a null or empty permission in the list.");
                        continue;
                    }

                    if (!Permissions.All.Contains(permission))
                    {
                        _logger.LogWarning("Tried to add undefined permission: {Permission}", permission);
                        failedPermissions.Add(permission);
                    }
                    else
                    {
                        if (existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            _logger.LogInformation("PermissionService - AddPermissionsToRoleAsync : Permission '{Permission}' already exists for role '{RoleName}'.", permission, roleName);
                            continue;
                        }

                        var claim = new Claim("Permission", permission);
                        var result = await _roleManager.AddClaimAsync(role, claim);

                        if (!result.Succeeded)
                        {
                            _logger.LogError("PermissionService - AddPermissionsToRoleAsync : Failed to add permission '{Permission}' to role '{RoleName}'. Errors: {Errors}", permission, roleName, result.Errors);
                            failedPermissions.Add(permission);
                        }
                    }
                }

                _logger.LogInformation("PermissionService - AddPermissionsToRoleAsync : Successfully added permissions to role.");
                return new GeneralResult(true, messages.MsgAddPermissionsPartialSuccess, failedPermissions, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService - AddPermissionsToRoleAsync : Unexpected error while adding permissions to role '{RoleName}'.", roleName);
                return new GeneralResult(false, messages.MsgUnexpectedAddPermissionsError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> RemovePermissionFromRoleAsync(string roleName, string permission)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("PermissionService - RemovePermissionFromRoleAsync : Role '{RoleName}' not found.", roleName);
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                var claim = new Claim("Permission", permission);
                var result = await _roleManager.RemoveClaimAsync(role, claim);

                if (!result.Succeeded)
                {
                    _logger.LogError("PermissionService - RemovePermissionFromRoleAsync : Failed to remove permission '{Permission}' from role '{RoleName}'. Errors: {Errors}", permission, roleName, result.Errors);
                    return new GeneralResult(false, messages.MsgRemovePermissionFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("PermissionService - RemovePermissionFromRoleAsync : Successfully removed permission '{Permission}' from role '{RoleName}'.", permission, roleName);
                return new GeneralResult(true, messages.MsgRemovePermissionSucceeded, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService - RemovePermissionFromRoleAsync : Unexpected error while removing permission '{Permission}' from role '{RoleName}'.", permission, roleName);
                return new GeneralResult(false, messages.MsgUnexpectedRemovePermissionError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<string>>> GetPermissionsForRoleAsync(string roleName)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("PermissionService - GetPermissionsForRoleAsync : Role '{RoleName}' not found.", roleName);
                    return new GeneralResult<List<string>>(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                var claims = await _roleManager.GetClaimsAsync(role);
                var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

                _logger.LogInformation("PermissionService - GetPermissionsForRoleAsync : Retrieved {Count} permissions for role '{RoleName}'.", permissions.Count, roleName);
                return new GeneralResult<List<string>>(true, messages.MsgPermissionsRetrievedForRole, permissions, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService - GetPermissionsForRoleAsync : Unexpected error while retrieving permissions for role '{RoleName}'.", roleName);
                return new GeneralResult<List<string>>(false, messages.MsgUnexpectedRetrievePermissionsError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<string>>> GetPermissionsForUserAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("PermissionService - GetPermissionsForUserAsync : User id cannot be null.");
                    return new GeneralResult<List<string>>(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("PermissionService - GetPermissionsForUserAsync : User '{UserId}' not found.", userId);
                    return new GeneralResult<List<string>>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var roles = await _userManager.GetRolesAsync(user.Data);
                var permissions = new List<string>();

                foreach (var role in roles)
                {
                    var roleClaims = await GetPermissionsForRoleAsync(role);
                    if (roleClaims.Data == null)
                    {
                        continue;
                    }

                    permissions.AddRange(roleClaims.Data);
                }

                var distinctPermissions = permissions.Distinct().ToList();
                _logger.LogInformation("PermissionService - GetPermissionsForUserAsync : Retrieved {Count} distinct permissions for user '{UserId}'.", distinctPermissions.Count, userId);
                return new GeneralResult<List<string>>(true, messages.MsgPermissionsRetrievedForUser, distinctPermissions, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService - GetPermissionsForUserAsync : Unexpected error while retrieving permissions for user '{UserId}'.", userId);
                return new GeneralResult<List<string>>(false, messages.MsgUnexpectedRetrieveUserPermissionsError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<bool>> UserHasPermissionAsync(string userId, CancellationToken cancellationToken, string permission)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("PermissionService - UserHasPermissionAsync : User id cannot be null.");
                    return new GeneralResult<bool>(false, messages.MsgIdInvalid, false, ErrorType.BadRequest);
                }

                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("PermissionService - UserHasPermissionAsync : User '{UserId}' not found.", userId);
                    return new GeneralResult<bool>(false, messages.MsgUserNotFound, false, ErrorType.NotFound);
                }

                var permissions = await GetPermissionsForUserAsync(user.Data.Id, cancellationToken);
                if (permissions.Data == null)
                {
                    _logger.LogInformation("PermissionService - UserHasPermission: User '{UserId}' does not have any permissions.", userId);
                    return new GeneralResult<bool>(true, messages.MsgUserHasNoPermissions, false);
                }

                var hasPermission = permissions.Data.Contains(permission);
                if(!hasPermission)
                {
                    _logger.LogInformation("PermissionService - UserHasPermission: User '{UserId}' does not have the permission '{Permission}'.", userId, permission);
                    return new GeneralResult<bool>(true, messages.MsgUserDoesNotHavePermission, false, ErrorType.Success);
                }

                _logger.LogInformation("PermissionService - UserHasPermission: User '{UserId}' has the permission '{Permission}'.", userId, permission);
                return new GeneralResult<bool>(true, messages.MsgUserHasPermission, hasPermission, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService - UserHasPermission: Unexpected error while checking permission '{Permission}' for user '{UserId}'.", permission, userId);
                return new GeneralResult<bool>(false, messages.MsgUnexpectedCheckPermissionError, false, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> RemovePermissionFromUserAsync(string userId, CancellationToken cancellationToken, string permission)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("PermissionService - RemovePermissionFromUserAsync : User id cannot be null.");
                    return new GeneralResult(false, messages.MsgIdInvalid, false, ErrorType.BadRequest);
                }

                var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
                if (user.Data == null)
                {
                    _logger.LogWarning("PermissionService - RemovePermissionFromUserAsync : User '{UserId}' not found.", userId);
                    return new GeneralResult(false, messages.MsgUserNotFound, false, ErrorType.NotFound);
                }

                if (string.IsNullOrWhiteSpace(permission))
                {
                    _logger.LogError("PermissionService - RemovePermissionFromUser: Permission cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgPermissionCannotBeEmpty, null, ErrorType.BadRequest);
                }

                var claims = await _userManager.GetClaimsAsync(user.Data);
                var claimToRemove = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permission);

                if (claimToRemove == null)
                {
                    _logger.LogWarning("PermissionService - RemovePermissionFromUser: User '{UserId}' does not have permission '{Permission}'.", userId, permission);
                    return new GeneralResult(false, messages.MsgUserHasNoPermissions, null, ErrorType.Success);
                }

                var result = await _userManager.RemoveClaimAsync(user.Data, claimToRemove);
                if (!result.Succeeded)
                {
                    _logger.LogError("PermissionService - RemovePermissionFromUser: Failed to remove permission '{Permission}' from user '{UserId}'. Errors: {Errors}", permission, userId, result.Errors);
                    return new GeneralResult(false,  $"RemovePermissionFromUser: Failed to remove permission '{permission}' from user '{userId}'. Errors: {result.Errors}." );
                }

                _logger.LogInformation("PermissionService - RemovePermissionFromUser: Successfully removed permission '{Permission}' from user '{UserId}'.", permission, userId);
                return new GeneralResult(true, messages.MsgRemovePermissionFromUserSucceeded, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService - RemovePermissionFromUser: Unexpected error while removing permission '{Permission}' from user '{UserId}'.", permission, userId);
                return new GeneralResult(false, messages.MsgUnexpectedRemovePermissionFromUserError, null, ErrorType.InternalServerError);
            }
        }

        //// HELPER METHODS
        //public async Task<bool> CanAccessEnrollmentAsync(string userId, ProgramEnrollment enrollment, CancellationToken cancellationToken)
        //{
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        _logger.LogError("PermissionService - CanAccessEnrollmentAsync : User id cannot be null.");
        //        return false;
        //    }

        //    var user = await userService.FindUserAsync(cancellationToken, null, null, userId, true);
        //    if (user.Data == null)
        //    {
        //        _logger.LogWarning("PermissionService - RemovePermissionFromUserAsync : User '{UserId}' not found.", userId);
        //        return false;
        //    }

        //    if (user.Data.Id == enrollment.UserId)
        //        return true;

        //    var permissionCheck = await UserHasPermissionAsync(userId, cancellationToken, Permissions.CertificatePermissions.AccessAllCertificates);
        //    return permissionCheck.Data;
        //}
    }
}
