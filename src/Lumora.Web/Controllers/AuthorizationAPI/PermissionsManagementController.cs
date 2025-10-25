using Lumora.DTOs.Authorization;
namespace Lumora.Web.Controllers.Authorization
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public class PermissionsManagementController(
    IPermissionService permissionService,
    UserManager<User> userManager,
    ILocalizationManager? localization,
    PermissionMessage messages,
    ILogger<PermissionsManagementController> logger) : AuthenticatedController
    {
        private readonly IPermissionService _permissionService = permissionService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILocalizationManager? _localization = localization;
        private readonly ILogger<PermissionsManagementController> _logger = logger;

        /// <summary>
        /// Assigns a specific permission to the specified role.
        /// </summary>
        /// <param name="dto">Data transfer object containing the role name and the permission identifier to be added.</param>
        /// <returns>
        /// Returns <see cref="IActionResult"/> indicating the result of the operation:
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> if the permission was successfully added.</description></item>
        /// <item><description><c>400 Bad Request</c> if the input is invalid.</description></item>
        /// <item><description><c>404 Not Found</c> if the role does not exist.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected failures.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("add-to/")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPermissionToRole([FromBody] AddPermissionDto dto)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _permissionService.AddPermissionToRoleAsync(dto.RoleName, dto.Permission);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding permission {Permission} to role {RoleName}.", dto.Permission, dto.RoleName);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" adding permission to role."), Data = null });
            }
        }

        /// <summary>
        /// Assigns multiple permissions to the specified role in a single operation.
        /// </summary>
        /// <param name="dto">Data transfer object containing the role name and the permissions to be added.</param>
        /// <returns>
        /// Returns <see cref="IActionResult"/> indicating the result of the operation:
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> if all permissions were successfully added.</description></item>
        /// <item><description><c>400 Bad Request</c> if any permission is invalid.</description></item>
        /// <item><description><c>404 Not Found</c> if the role does not exist.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected failures.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("add-multiple-to/")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddPermissionsToRole([FromBody] AddMultiplePermissionsDto dto)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _permissionService.AddPermissionsToRoleAsync(dto.RoleName, dto.Permissions);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding multiple permissions to role {RoleName}.", dto.RoleName);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" adding multiple permissions to role."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves all permissions associated with a specific role.
        /// </summary>
        /// <param name="roleName">The unique name of the role whose permissions are to be retrieved. Must be passed via query string.</param>
        /// <returns>
        /// Returns <see cref="IActionResult"/> containing a list of permissions:
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with the permissions list.</description></item>
        /// <item><description><c>400 Bad Request</c> if the input is invalid.</description></item>
        /// <item><description><c>404 Not Found</c> if the role does not exist.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected failures.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("roles/")]
        [ProducesResponseType(typeof(GeneralResult<List<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPermissionsForRole([FromQuery] string roleName)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _permissionService.GetPermissionsForRoleAsync(roleName);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for role {RoleName}.", roleName);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" get permissions for role."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves all permissions assigned to a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user. Must be passed via query string.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// Returns <see cref="IActionResult"/> containing a list of user permissions:
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with the permissions list.</description></item>
        /// <item><description><c>400 Bad Request</c> if the input is invalid.</description></item>
        /// <item><description><c>404 Not Found</c> if the user does not exist.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected failures.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("get-by-user/")]
        [ProducesResponseType(typeof(GeneralResult<List<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPermissionsForUser([FromQuery] string userId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _permissionService.GetPermissionsForUserAsync(userId, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for user {UserId}.", userId);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" get permissions for user."), Data = null });
            }
        }

        /// <summary>
        /// Verifies whether a specific user has a given permission.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to check.</param>
        /// <param name="permission">The permission identifier to be validated for the user. Must be passed via query string.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// Returns <see cref="IActionResult"/> with a boolean indicating permission status:
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with <c>true</c> or <c>false</c> for permission status.</description></item>
        /// <item><description><c>404 Not Found</c> if the user does not exist.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected failures.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("check-user")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckUserPermission([FromQuery] string userId, [FromQuery] string permission, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found with ID {UserId}.", userId);
                    return NotFound(new { Message = _localization!.GetLocalizedString("UserNotFound") });
                }

                var hasPermission = await _permissionService.UserHasPermissionAsync(userId, cancellationToken, permission);
                _logger.LogInformation("Permission check for user {UserId} with permission {Permission}: {Result}", userId, permission, hasPermission);
                return Ok(new { HasPermission = hasPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}.", permission, userId);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" check permission for user."), Data = null });
            }
        }

        /// <summary>
        /// Removes a specific permission from the specified role.
        /// </summary>
        /// <param name="dto">Data transfer object containing the role name and the permission identifier to be removed.</param>
        /// <returns>
        /// Returns <see cref="IActionResult"/> indicating the result of the operation:
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> if the permission was successfully removed.</description></item>
        /// <item><description><c>400 Bad Request</c> if the input is invalid.</description></item>
        /// <item><description><c>404 Not Found</c> if the role or permission does not exist.</description></item>
        /// <item><description><c>500 Internal Server Error</c> for unexpected failures.</description></item>
        /// </list>
        /// </returns>
        [HttpDelete("remove-from-role/")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> RemovePermissionFromRole([FromBody] RemovePermissionDto dto)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _permissionService.RemovePermissionFromRoleAsync(dto.RoleName, dto.Permission);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission {Permission} from role {RoleName}.", dto.Permission, dto.RoleName);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" removing permission from role."), Data = null });
            }
        }
    }
}
