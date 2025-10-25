namespace Lumora.Web.Controllers.Authorization
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public class RolesManagementController(
        IRoleService roleService,
        UserManager<User> userManager,
        RoleMessages messages,
        ILogger<RolesManagementController> logger) : AuthenticatedController
    {
        private readonly IRoleService _roleService = roleService;
        protected readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<RolesManagementController> _logger = logger;

        // ===== Roles Management =====

        /// <summary>
        /// Creates and ensures the existence of default system roles in the identity store.
        /// This method is intended to be called once during system setup or recovery.
        /// </summary>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with success if roles are ensured or already exist,
        /// otherwise returns the appropriate error status code and details.
        /// </returns>
        [HttpPost("default-roles")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnsureDefaultRoles()
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _roleService.EnsureSeedRolesAsync();
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
                _logger.LogError(ex, "Error ensuring default roles.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" ensuring default roles"), Data = null });
            }
        }

        /// <summary>
        /// Retrieves a complete list of all roles defined in the system.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation if requested.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> containing the list of roles on success,
        /// or an appropriate error result if retrieval fails.
        /// </returns>
        [HttpGet("all")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _roleService.GetAllRolesAsync(cancellationToken);
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
                _logger.LogError(ex, "Error retrieving all roles.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" retrieving all roles"), Data = null });
            }
        }

        // ===== Role Assignment to Users =====

        /// <summary>
        /// Assigns a specified role to a user.
        /// </summary>
        /// <param name="dto">Data transfer object containing the target user's ID and the role name to assign.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> if the role is successfully assigned,
        /// or an appropriate error result if the operation fails.
        /// </returns>
        [HttpPost("assign")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _roleService.AssignRoleAsync(dto.UserId, dto.Role);
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
                _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}.", dto.Role, dto.UserId);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" assigning role"), Data = null });
            }
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="dto">Data transfer object containing the user's ID and the role name to remove.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> if the role is successfully removed from the user,
        /// or an appropriate error result if the operation fails.
        /// </returns>
        [HttpPost("remove-role")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _roleService.RemoveRoleAsync(dto.UserId, dto.Role);
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
                _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}.", dto.Role, dto.UserId);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" removing role"), Data = null });
            }
        }

        /// <summary>
        /// Retrieves a list of users assigned to the specified role.
        /// </summary>
        /// <param name="roleName">The name of the role for which users should be retrieved.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with the list of users in the role,
        /// or an appropriate error result if the operation fails.
        /// </returns>
        [HttpGet("role-users")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersInRole([FromQuery] string roleName)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _roleService.GetUsersInRoleAsync(roleName);
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
                _logger.LogError(ex, "Error retrieving users in role {RoleName}.", roleName);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" retrieving users in role"), Data = null });
            }
        }

        // ===== Role Verification and Lookup =====

        /// <summary>
        /// Checks whether a role with the specified name exists in the system.
        /// </summary>
        /// <param name="roleName">The name of the role to check for existence.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with a boolean result indicating role existence,
        /// or an appropriate error result if the operation fails.
        /// </returns>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RoleExists([FromQuery] string roleName)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _roleService.RoleExistsAsync(roleName);
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
                _logger.LogError(ex, "Error checking role existence for {RoleName}.", roleName);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" checking role existence"), Data = null });
            }
        }

        /// <summary>
        /// Determines whether a specific user is assigned to a particular role.
        /// </summary>
        /// <param name="dto">Data transfer object containing the user's ID and the role name to check.</param>
        /// <param name="cancellationToken">Token to cancel the request if needed.</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with a boolean result indicating membership,
        /// or an appropriate error result if the check fails.
        /// </returns>
        [HttpGet("is-in-role")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IsUserInRole([FromQuery] UserInRole dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _roleService.IsUserInRoleAsync(dto.UserId, dto.Role, cancellationToken);
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
                _logger.LogError(ex, "Error checking if user {UserId} is in role {RoleName}.", dto.UserId, dto.Role);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" checking if user is in role"), Data = null });
            }
        }
    }
}
