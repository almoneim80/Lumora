namespace Lumora.Application.Services.Authorization
{
    public class PermissionService(
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        ILogger<PermissionService> logger,
        PermissionMessage messages) : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository = permissionRepository;
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly IUserRepository _userRepository = userRepository;
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

                // Domain rule validation using static metadata
                if (!Permissions.All.Contains(permission))
                {
                    _logger.LogWarning("Tried to add undefined permission: {Permission}", permission);
                    return new GeneralResult(false, messages.MsgPermissionNotDefined, null, ErrorType.NotFound);
                }

                // Retrieve role through repository abstraction
                var role = await _permissionRepository.FindRoleByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("PermissionService - AddPermissionToRoleAsync : Role '{RoleName}' not found.", roleName);
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                // Check for existing permission using repository abstraction
                var hasPermission = await _permissionRepository.RoleHasPermissionAsync(role, permission);
                if (hasPermission)
                {
                    _logger.LogInformation("PermissionService - AddPermissionToRoleAsync : Permission '{Permission}' already exists for role '{RoleName}'.", permission, roleName);
                    return new GeneralResult(false, messages.MsgPermissionAlreadyExistsForRole, null, ErrorType.BadRequest);
                }

                // Execute operation through repository
                var result = await _permissionRepository.AddPermissionToRoleAsync(role, permission);

                if (!result.Succeeded)
                {
                    _logger.LogError("PermissionService - AddPermissionToRoleAsync : Failed to add permission '{Permission}' to role '{RoleName}'. Errors: {Errors}", permission, roleName, string.Join(", ", result.Errors));
                    return new GeneralResult(false, messages.MsgAddPermissionFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("PermissionService - AddPermissionToRoleAsync : Successfully added permission '{Permission}' to role '{RoleName}'.", permission, roleName);
                return new GeneralResult(true, messages.MsgAddPermissionSucceeded, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PermissionService - AddPermissionToRoleAsync : Unexpected error while adding permission '{Permission}' to role '{RoleName}'.", permission, roleName);
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

                // Fetch role via repository to keep Application Layer clean from Identity dependencies
                var role = await _permissionRepository.FindRoleByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("PermissionService - AddPermissionsToRoleAsync : AddPermissionsToRole: Role '{RoleName}' not found.", roleName);
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                var existingClaims = await _permissionRepository.GetRoleClaimsAsync(role);
                var distinctPermissions = permissions.Distinct();
                var failedPermissions = new List<string>();

                foreach (var permission in distinctPermissions)
                {
                    if (string.IsNullOrWhiteSpace(permission))
                    {
                        _logger.LogWarning("PermissionService - AddPermissionsToRoleAsync : AddPermissionsToRole: Encountered a null or empty permission in the list.");
                        continue;
                    }

                    // Check if permission is defined in the system constants
                    if (!Permissions.All.Contains(permission))
                    {
                        _logger.LogWarning("Tried to add undefined permission: {Permission}", permission);
                        failedPermissions.Add(permission);
                        continue;
                    }

                    // Avoid duplicate claims
                    if (existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                    {
                        _logger.LogInformation("PermissionService - AddPermissionsToRoleAsync : Permission '{Permission}' already exists for role '{RoleName}'.", permission, roleName);
                        continue;
                    }

                    var claim = new Claim("Permission", permission);
                    var result = await _permissionRepository.AddClaimToRoleAsync(role, claim);

                    if (!result.Succeeded)
                    {
                        _logger.LogError("PermissionService - AddPermissionsToRoleAsync : Failed to add permission '{Permission}' to role '{RoleName}'. Errors: {Errors}",
                            permission, roleName, string.Join(", ", result.Errors));
                        failedPermissions.Add(permission);
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
                // Use repository to find the domain entity instead of RoleManager
                var role = await _permissionRepository.FindRoleByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("PermissionService - RemovePermissionFromRoleAsync : Role '{RoleName}' not found.", roleName);
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                var claim = new Claim("Permission", permission);

                // Execute the removal operation via the repository abstraction
                var result = await _permissionRepository.RemoveClaimFromRoleAsync(role, claim);

                if (!result.Succeeded)
                {
                    _logger.LogError("PermissionService - RemovePermissionFromRoleAsync : Failed to remove permission '{Permission}' from role '{RoleName}'. Errors: {Errors}",
                        permission, roleName, string.Join(", ", result.Errors));

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
                // Use repository to find role existence and get filtered claims
                var permissions = await _permissionRepository.GetRoleClaimsValuesByTypeAsync(roleName, "Permission");

                // Checking if role exists through the result since the repo returns empty list for non-existent roles
                // To be more precise, we check the role explicitly to return the correct error message
                var roleExists = await _permissionRepository.FindRoleByNameAsync(roleName);

                if (roleExists == null)
                {
                    _logger.LogWarning("PermissionService - GetPermissionsForRoleAsync : Role '{RoleName}' not found.", roleName);
                    return new GeneralResult<List<string>>(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

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

                var userResult = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (userResult == null)
                {
                    _logger.LogWarning("PermissionService - GetPermissionsForUserAsync : User '{UserId}' not found.", userId);
                    return new GeneralResult<List<string>>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // Using RoleRepository to abstract Identity framework
                var roles = await _roleRepository.GetUserRolesAsync(userResult);
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

                /* Fetch user entity directly from the repository to maintain data integrity */
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("PermissionService - UserHasPermissionAsync : User '{UserId}' not found.", userId);
                    return new GeneralResult<bool>(false, messages.MsgUserNotFound, false, ErrorType.NotFound);
                }

                /* Aggregate user permissions from claims and roles */
                var userClaims = await _permissionRepository.GetUserClaimsAsync(user);
                var userRoles = await _permissionRepository.GetUserRolesAsync(user);

                var allPermissions = userClaims.Select(c => c.Value).ToList();

                foreach (var roleName in userRoles)
                {
                    var role = await _permissionRepository.FindRoleByNameAsync(roleName);
                    if (role != null)
                    {
                        var roleClaims = await _permissionRepository.GetRoleClaimsAsync(role);
                        allPermissions.AddRange(roleClaims.Select(c => c.Value));
                    }
                }

                if (!allPermissions.Any())
                {
                    _logger.LogInformation("PermissionService - UserHasPermission: User '{UserId}' does not have any permissions.", userId);
                    return new GeneralResult<bool>(true, messages.MsgUserHasNoPermissions, false);
                }

                var hasPermission = allPermissions.Contains(permission);
                if (!hasPermission)
                {
                    _logger.LogInformation("PermissionService - UserHasPermission: User '{UserId}' does not have the permission '{Permission}'.", userId, permission);
                    return new GeneralResult<bool>(true, messages.MsgUserDoesNotHavePermission, false, ErrorType.Success);
                }

                _logger.LogInformation("PermissionService - UserHasPermission: User '{UserId}' has the permission '{Permission}'.", userId, permission);
                return new GeneralResult<bool>(true, messages.MsgUserHasPermission, true, ErrorType.Success);
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

                /* Use the repository to fetch the user instead of a high-level service to avoid circular dependency or overhead */
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("PermissionService - RemovePermissionFromUserAsync : User '{UserId}' not found.", userId);
                    return new GeneralResult(false, messages.MsgUserNotFound, false, ErrorType.NotFound);
                }

                if (string.IsNullOrWhiteSpace(permission))
                {
                    _logger.LogError("PermissionService - RemovePermissionFromUser: Permission cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgPermissionCannotBeEmpty, null, ErrorType.BadRequest);
                }

                /* Abstracting Identity-specific Claim logic through the Permission Repository */
                var claims = await _permissionRepository.GetUserClaimsAsync(user);
                var claimToRemove = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permission);

                if (claimToRemove == null)
                {
                    _logger.LogWarning("PermissionService - RemovePermissionFromUser: User '{UserId}' does not have permission '{Permission}'.", userId, permission);
                    return new GeneralResult(false, messages.MsgUserHasNoPermissions, null, ErrorType.Success);
                }

                /* Execute the removal through the repository and handle the result as an OperationResult */
                var result = await _permissionRepository.RemoveClaimFromUserAsync(user, claimToRemove);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger.LogError("PermissionService - RemovePermissionFromUser: Failed to remove permission '{Permission}' from user '{UserId}'. Errors: {Errors}", permission, userId, errors);
                    return new GeneralResult(false, $"RemovePermissionFromUser: Failed to remove permission '{permission}' from user '{userId}'. Errors: {errors}.", null, ErrorType.InternalServerError);
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
    }
}
