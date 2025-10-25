using Lumora.Services.Core.Messages;

namespace Lumora.Services.Authorization
{
    public class RoleService(
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager,
        RoleMessages messages,
        IConfiguration configuration,
        ILogger<RoleService> logger) : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<RoleService> _logger = logger;

        /// <inheritdoc/>
        public async Task<GeneralResult> EnsureSeedRolesAsync()
        {
            try
            {
                var defaultRoles = _configuration.GetSection("DefaultRoles").Get<DefaultRolesConfig>() ?? new DefaultRolesConfig();
                var createdRoles = new List<string>();
                foreach (var role in defaultRoles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        var result = await _roleManager.CreateAsync(new IdentityRole(role));
                        if (!result.Succeeded)
                        {
                            _logger.LogError($"RoleService - EnsureSeedRolesAsync : Failed to create role {role}.");
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
                var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync(cancellationToken);

                if (!roles.Any())
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

                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogInformation($"RoleService - AddRoleAsync : Role {roleName} already exists.");
                    return new GeneralResult(true, messages.MsgRoleAlreadyExists, null, ErrorType.Success);
                }

                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
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

                _logger.LogInformation($"RoleService - RoleExistsAsync : Checking existence of role {roleName}.");
                var result = await _roleManager.RoleExistsAsync(roleName);
                if (!result)
                {
                    _logger.LogInformation($"RoleService - RoleExistsAsync : Role {roleName} does not exist.");
                    return new GeneralResult<bool>(false, messages.MsgRoleNotFound, result);
                }

                _logger.LogInformation($"RoleService - RoleExistsAsync : Role {roleName} exists.");
                return new GeneralResult<bool>(true, messages.MsgRoleExists, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - RoleExistsAsync : Error checking existence of role {roleName}.");
                return new GeneralResult<bool>(false, messages.MsgRoleExistenceCheckError, false, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> AssignRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
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

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    _logger.LogError($"RoleService - AssignRoleAsync : Role {role} does not exist.");
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                var result = await _userManager.AddToRoleAsync(user, role);
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
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogError($"Role '{roleName}' does not exist.");
                    return new GeneralResult(false, messages.MsgDeleteRoleNotFound, null, ErrorType.BadRequest);
                }

                var usersInRole = await GetUsersInRoleAsync(roleName);
                if (usersInRole.IsSuccess && usersInRole.Data?.Any() == true)
                {
                    _logger.LogWarning($"RoleService - DeleteRoleAsync : Role '{roleName}' cannot be deleted because it is assigned to one or more users.");
                    return new GeneralResult(false, messages.MsgDeleteRoleAssigned, null, ErrorType.BadRequest);
                }

                if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"RoleService - DeleteRoleAsync : Role '{roleName}' cannot be deleted.");
                    return new GeneralResult(false, messages.MsgDeleteAdminRole, null, ErrorType.BadRequest);
                }

                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    _logger.LogError($"RoleService - DeleteRoleAsync : Failed to delete role {roleName}: {result.Errors}");
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

                var role = await _roleManager.FindByNameAsync(oldRoleName);
                if (role == null)
                {
                    _logger.LogError($"RoleService - UpdateRoleNameAsync : Role '{oldRoleName}' does not exist.");
                    return new GeneralResult(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                if (await _roleManager.RoleExistsAsync(newRoleName))
                {
                    _logger.LogWarning($"RoleService - UpdateRoleNameAsync : Role '{newRoleName}' already exists.");
                    return new GeneralResult(false, messages.MsgRoleExists, null, ErrorType.BadRequest);
                }

                role.Name = newRoleName;
                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    string errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogError("RoleService - UpdateRoleNameAsync : Failed to update role name from" + $"{oldRoleName} " + $"to" + $"{newRoleName}", errors);
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
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogError($"RoleService - RemoveRoleAsync : User {userId} not found or deleted.");
                    return new GeneralResult(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (string.IsNullOrWhiteSpace(role))
                {
                    _logger.LogError("RoleService - RemoveRoleAsync : Role name cannot be null or empty.");
                    return new GeneralResult(false, messages.MsgRoleNameEmpty, null, ErrorType.BadRequest);
                }

                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    _logger.LogError($"RoleService - RemoveRoleAsync : Failed to remove role {role} from user {userId}: {result.Errors}");
                    return new GeneralResult(false, messages.MsgRemoveRoleFailed, null, ErrorType.BadRequest);
                }

                _logger.LogInformation($"RoleService - RemoveRoleAsync : Role {role} removed from user {userId} successfully.");
                return new GeneralResult(true, messages.MsgRemoveRoleSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - RemoveRoleAsync : Error removing role {role} from user {userId}.");
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

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogError($"RoleService - GetUsersInRoleAsync : Role '{roleName}' does not exist.");
                    return new GeneralResult<List<User>>(false, messages.MsgRoleNotFound, null, ErrorType.NotFound);
                }

                var users = await _userManager.GetUsersInRoleAsync(roleName);
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
                var user = await _userManager.Users.FirstOrDefaultAsync(
                    usr => usr.Id == userId && !usr.IsDeleted && usr.IsActive, cancellationToken);
                if (user == null)
                {
                    _logger.LogError($"RoleService - IsUserInRoleAsync : User {userId} not found.");
                    return new GeneralResult<bool>(false, messages.MsgUserNotFound, false, ErrorType.NotFound);
                }

                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogError("RoleService - IsUserInRoleAsync : Role name cannot be null or empty.");
                    return new GeneralResult<bool>(false, messages.MsgRoleNameEmpty, false, ErrorType.BadRequest);
                }

                var result = await _userManager.IsInRoleAsync(user, roleName);
                if (!result)
                {
                    _logger.LogWarning($"RoleService - IsUserInRoleAsync : User {userId} is not in role {roleName}.");
                    return new GeneralResult<bool>(false, messages.MsgUserNotInRole, false, ErrorType.NotFound);
                }

                _logger.LogInformation($"RoleService - IsUserInRoleAsync : user with id {userId} is in role {roleName}.");
                return new GeneralResult<bool>(true, messages.MsgUserInRole, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RoleService - IsUserInRoleAsync : Error checking if user {userId} is in role {roleName}.");
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

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
                if (user == null)
                {
                    _logger.LogError($"RoleService - GetUserRolesAsync : User with ID '{userId}' not found.");
                    return new GeneralResult<List<string>>(false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var roles = await _userManager.GetRolesAsync(user);
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
