using Microsoft.AspNetCore.Identity;

namespace Lumora.Application.Services.Authorization
{
    public class RoleService(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        RoleMessages messages,
        ILogger<RoleService> logger,
        IOptions<RoleSettings> settings) : IRoleService
    {
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ILogger<RoleService> _logger = logger;
        private readonly RoleSettings _roleSettings = settings.Value;

        /// <inheritdoc/>
        public async Task<GeneralResult> EnsureSeedRolesAsync()
        {
            try
            {
                var defaultRoles = _roleSettings.DefaultRoles;
                var createdRoles = new List<string>();

                foreach (var role in defaultRoles)
                {
                    /* Check existence through repository instead of Identity managers */
                    if (!await _roleRepository.RoleExistsAsync(role))
                    {
                        /* Create role through repository abstraction */
                        var result = await _roleRepository.CreateRoleAsync(role);

                        if (!result.Succeeded)
                        {
                            _logger.LogError("RoleService - EnsureSeedRolesAsync : Failed to create role {role}.", role);
                            return new GeneralResult(false, messages.MsgRoleCreationFailed, null, ErrorType.BadRequest);
                        }

                        createdRoles.Add(role);
                    }
                }

                if (createdRoles.Any())
                {
                    var message = $"Created roles: {string.Join(", ", createdRoles)}";
                    _logger.LogInformation("RoleService - EnsureSeedRolesAsync : " + message);
                    return new GeneralResult(true, messages.MsgRolesCreated, null, ErrorType.Success);
                }

                _logger.LogInformation("RoleService - EnsureSeedRolesAsync : Default roles already exist.");
                return new GeneralResult(true, messages.MsgRoleAlreadyExists, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleService - EnsureSeedRolesAsync : Error ensuring default roles.");
                return new GeneralResult(false, messages.MsgDefaultRolesError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<string>>> GetAllRolesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("RoleService - GetAllRolesAsync : Retrieving all roles.");

                /* Retrieve roles through the abstraction layer */
                var roles = await _roleRepository.GetAllRoleNamesAsync(cancellationToken);

                if (roles == null || roles.Count == 0)
                {
                    _logger.LogWarning("RoleService - GetAllRolesAsync : No roles found.");
                    return new GeneralResult<List<string>>(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                _logger.LogInformation("RoleService - GetAllRolesAsync : {Count} roles found.", roles.Count);
                return new GeneralResult<List<string>>(true, messages.MsgRolesRetrieved, roles, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleService - GetAllRolesAsync : Error retrieving all roles.");
                return new GeneralResult<List<string>>(false, messages.MsgRolesRetrievalError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> AddRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogError("RoleService - AddRoleAsync : Role name cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgRoleNameEmpty, null, ErrorType.BadRequest);
                }

                // Check role existence via repository
                if (await _roleRepository.RoleExistsAsync(roleName))
                {
                    _logger.LogInformation($"RoleService - AddRoleAsync : Role {roleName} already exists.");
                    return new GeneralResult(true, messages.MsgRoleAlreadyExists, null, ErrorType.Success);
                }

                // Create new role via repository
                var result = await _roleRepository.CreateRoleAsync(roleName);

                if (!result.Succeeded)
                {
                    _logger.LogError($"RoleService - AddRoleAsync : Failed to add role {roleName}.");
                    return new GeneralResult(false, messages.MsgAddRoleFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation($"RoleService - AddRoleAsync : Role {roleName} added successfully.");
                return new GeneralResult(true, messages.MsgAddRoleSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - AddRoleAsync : Error adding role {roleName}.");
                return new GeneralResult(false, messages.MsgAddRoleError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<bool>> RoleExistsAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogError("RoleService - RoleExistsAsync : Role name cannot be null or empty.");
                    return new GeneralResult<bool>(false, messages.MsgRoleNameEmpty, false);
                }

                _logger.LogInformation("RoleService - RoleExistsAsync : Checking existence of role {RoleName}.", roleName);

                // Interaction with the repository instead of direct Infrastructure managers
                var result = await _roleRepository.RoleExistsAsync(roleName);

                if (!result)
                {
                    _logger.LogInformation("RoleService - RoleExistsAsync : Role {RoleName} does not exist.", roleName);
                    return new GeneralResult<bool>(false, messages.MsgRoleNotFound, result);
                }

                _logger.LogInformation("RoleService - RoleExistsAsync : Role {RoleName} exists.", roleName);
                return new GeneralResult<bool>(true, messages.MsgRoleExists, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleService - RoleExistsAsync : Error checking existence of role {RoleName}.", roleName);
                return new GeneralResult<bool>(false, messages.MsgRoleExistenceCheckError, false, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> AssignRoleAsync(string userId, string role)
        {
            try
            {
                /* Fetch user through repository instead of direct UserManager access */
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    _logger.LogError($"RoleService - AssignRoleAsync : User {userId} not found or inactive.");
                    return new GeneralResult(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (string.IsNullOrWhiteSpace(role))
                {
                    _logger.LogError("RoleService - AssignRoleAsync : Role name cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgRoleNameEmpty, null, ErrorType.BadRequest);
                }

                /* Verify role existence via repository */
                if (!await _roleRepository.RoleExistsAsync(role))
                {
                    _logger.LogError($"RoleService - AssignRoleAsync : Role {role} does not exist.");
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                /* Perform role assignment operation through repository abstraction */
                var result = await _roleRepository.AddToRoleAsync(user, role);

                if (!result.Succeeded)
                {
                    _logger.LogError($"RoleService - AssignRoleAsync : Failed to assign role {role} to user {userId}.");
                    return new GeneralResult(false, messages.MsgAssignRoleFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation($"RoleService - AssignRoleAsync : Role {role} assigned to user {userId} successfully.");
                return new GeneralResult(true, messages.MsgAssignRoleSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - AssignRoleAsync : Error assigning role {role} to user {userId}.");
                return new GeneralResult(false, messages.MsgAssignRoleError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            try
            {
                // Fetch role via repository instead of direct manager access
                var role = await _roleRepository.FindRoleByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogError($"Role '{roleName}' does not exist.");
                    return new GeneralResult(false, messages.MsgDeleteRoleNotFound, null, ErrorType.BadRequest);
                }

                // Business Rule: Check for users assigned to this role using repository abstraction
                var usersInRole = await _roleRepository.GetUsersInRoleAsync(roleName);
                if (usersInRole != null && usersInRole.Any())
                {
                    _logger.LogWarning($"RoleService - DeleteRoleAsync : Role '{roleName}' cannot be deleted because it is assigned to one or more users.");
                    return new GeneralResult(false, messages.MsgDeleteRoleAssigned, null, ErrorType.BadRequest);
                }

                // Business Rule: Protect critical system roles
                if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"RoleService - DeleteRoleAsync : Role '{roleName}' cannot be deleted.");
                    return new GeneralResult(false, messages.MsgDeleteAdminRole, null, ErrorType.BadRequest);
                }

                // Execute deletion via repository
                var result = await _roleRepository.DeleteRoleAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger.LogError($"RoleService - DeleteRoleAsync : Failed to delete role {roleName}: {errors}");
                    return new GeneralResult(false, messages.MsgDeleteRoleFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation($"RoleService - DeleteRoleAsync : Role {roleName} deleted successfully.");
                return new GeneralResult(true, messages.MsgDeleteRoleSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - DeleteRoleAsync : Error deleting role {roleName}.");
                return new GeneralResult(false, messages.MsgDeleteRoleError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateRoleNameAsync(string oldRoleName, string newRoleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldRoleName) || string.IsNullOrWhiteSpace(newRoleName))
                {
                    _logger.LogError("RoleService - UpdateRoleNameAsync : Role name cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgRoleNameEmpty, null, ErrorType.BadRequest);
                }

                // Fetching role through the repository abstraction
                var role = await _roleRepository.FindRoleByNameAsync(oldRoleName);
                if (role == null)
                {
                    _logger.LogError($"RoleService - UpdateRoleNameAsync : Role '{oldRoleName}' does not exist.");
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                // Validating uniqueness of the new role name
                if (await _roleRepository.RoleExistsAsync(newRoleName))
                {
                    _logger.LogWarning($"RoleService - UpdateRoleNameAsync : Role '{newRoleName}' already exists.");
                    return new GeneralResult(false, messages.MsgRoleExists, null, ErrorType.BadRequest);
                }

                role.Name = newRoleName;

                // Executing update through the repository
                var result = await _roleRepository.UpdateRoleAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger.LogError("RoleService - UpdateRoleNameAsync : Failed to update role name from " + $"{oldRoleName} " + "to " + $"{newRoleName}. Errors: {errors}");
                    return new GeneralResult(false, messages.MsgUpdateRoleNameFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation($"RoleService - UpdateRoleNameAsync : Role name updated from {oldRoleName} to {newRoleName} successfully.");
                return new GeneralResult(true, messages.MsgUpdateRoleNameSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - UpdateRoleNameAsync : Error updating role name from {oldRoleName} to {newRoleName}.");
                return new GeneralResult(false, messages.MsgUpdateRoleNameError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> RemoveRoleAsync(string userId, string role)
        {
            try
            {
                /* Fetch user through the repository abstraction */
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null || user.IsDeleted)
                {
                    _logger.LogError("RoleService - RemoveRoleAsync : User {UserId} not found or marked as deleted.", userId);
                    return new GeneralResult(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (string.IsNullOrWhiteSpace(role))
                {
                    _logger.LogError("RoleService - RemoveRoleAsync : Role name cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgRoleNameEmpty, null, ErrorType.BadRequest);
                }

                /* Perform role removal using the repository */
                var result = await _roleRepository.RemoveFromRoleAsync(user, role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger.LogError("RoleService - RemoveRoleAsync : Failed to remove role {Role} from user {UserId}. Errors: {Errors}", role, userId, errors);
                    return new GeneralResult(false, messages.MsgRemoveRoleFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation("RoleService - RemoveRoleAsync : Role {Role} removed from user {UserId} successfully.", role, userId);
                return new GeneralResult(true, messages.MsgRemoveRoleSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleService - RemoveRoleAsync : Unexpected error removing role {Role} from user {UserId}.", role, userId);
                return new GeneralResult(false, messages.MsgRemoveRoleError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<User>>> GetUsersInRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogError("RoleService - GetUsersInRoleAsync : Role name cannot be null or empty.");
                    return new GeneralResult<List<User>>(false, messages.MsgRoleNameEmpty, null, ErrorType.BadRequest);
                }

                // Use repository to check if the role exists
                var roleExists = await _roleRepository.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    _logger.LogError($"RoleService - GetUsersInRoleAsync : Role '{roleName}' does not exist.");
                    return new GeneralResult<List<User>>(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                // Use repository to retrieve users assigned to the specified role
                var users = await _roleRepository.GetUsersInRoleAsync(roleName);

                _logger.LogInformation($"RoleService - GetUsersInRoleAsync : Getting users in role {roleName}.");
                return new GeneralResult<List<User>>(true, messages.MsgUsersInRoleRetrieved, users.ToList(), ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - GetUsersInRoleAsync : Error getting users in role {roleName}.");
                return new GeneralResult<List<User>>(false, messages.MsgUsersInRoleError, null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<bool>> IsUserInRoleAsync(string userId, string roleName, CancellationToken cancellationToken)
        {
            try
            {
                /* Get the user through the repository abstracting the data source */
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

                if (user == null)
                {
                    _logger.LogError("RoleService - IsUserInRoleAsync : User {UserId} not found.", userId);
                    return new GeneralResult<bool>(false, messages.MsgUserNotFound, false, ErrorType.NotFound);
                }

                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogError("RoleService - IsUserInRoleAsync : Role name cannot be null or empty.");
                    return new GeneralResult<bool>(false, messages.MsgRoleNameEmpty, false, ErrorType.BadRequest);
                }

                /* Verify role membership using the repository implementation */
                var isInRole = await _roleRepository.IsInRoleAsync(user, roleName);

                if (!isInRole)
                {
                    _logger.LogWarning("RoleService - IsUserInRoleAsync : User {UserId} is not in role {RoleName}.", userId, roleName);
                    return new GeneralResult<bool>(false, messages.MsgUserNotInRole, false, ErrorType.NotFound);
                }

                _logger.LogInformation("RoleService - IsUserInRoleAsync : User with id {UserId} is in role {RoleName}.", userId, roleName);
                return new GeneralResult<bool>(true, messages.MsgUserInRole, true, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RoleService - IsUserInRoleAsync : Error checking if user {UserId} is in role {RoleName}.", userId, roleName);
                return new GeneralResult<bool>(false, messages.MsgUserInRoleCheckError, false, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<string>>> GetUserRolesAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogError("RoleService - GetUserRolesAsync : User ID cannot be null or empty.");
                    return new GeneralResult<List<string>>(false, messages.MsgUserIdEmpty, null, ErrorType.BadRequest);
                }

                /* Retrieve user through repository abstraction */
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogError($"RoleService - GetUserRolesAsync : User with ID '{userId}' not found or is deleted.");
                    return new GeneralResult<List<string>>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                /* Get roles using repository implementation */
                var roles = await _roleRepository.GetUserRolesAsync(user);

                _logger.LogInformation($"RoleService - GetUserRolesAsync : Retrieved roles for user {userId}.");
                return new GeneralResult<List<string>>(true, messages.MsgUserRolesRetrieved, roles.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - GetUserRolesAsync : Error retrieving roles for user {userId}.");
                return new GeneralResult<List<string>>(false, messages.MsgUserRolesRetrievalError, null, ErrorType.InternalServerError);
            }
        }
    }
}
