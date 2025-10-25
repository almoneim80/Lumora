using Lumora.Web.Controllers.Authentication;

namespace Lumora.Web.Controllers.AuthenticationAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public class UsersAdminController(
            IMapper mapper,
            UserManager<User> userManager,
            ILogger<AuthenticationController> logger,
            IUserService userService,
            IAuthenticationService authenticationService,
            GeneralMessage messages) : AuthenticatedController
    {
        protected readonly IMapper _mapper = mapper;
        protected readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<AuthenticationController> _logger = logger;
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Deactivates the currently authenticated user's account, preventing future logins.
        /// </summary>
        /// <param name="userId">The identifier of the user account to be deactivated.</param>
        /// <param name="reasone">The reason for deactivating the user account.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the account was deactivated successfully.
        /// Returns 400 Bad Request if the request is malformed or invalid.
        /// Returns 404 Not Found if the user does not exist.
        /// Returns 500 Internal Server Error if an unexpected error occurs during the deactivation process.
        /// </returns>
        [HttpPost("deactivate")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateUser([FromQuery] string userId, string reasone, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _authenticationService.DeactivateUserAsync(userId, cancellationToken, reasone);
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
                _logger.LogError(ex, "Deactivate-User - An Unexpected error occurred while deactivate user.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("deactivate user."), Data = null });
            }
        }

        /// <summary>
        /// Reactivates a previously deactivated user account.
        /// </summary>
        /// <param name="userId">The identifier of the user account to be reactivated.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the account was activated successfully.
        /// Returns 400 Bad Request if the userId is invalid or activation fails.
        /// Returns 404 Not Found if the user does not exist.
        /// Returns 500 Internal Server Error if an unexpected error occurs during activation.
        /// </returns>
        [HttpPost("activate")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivateUser([FromQuery] string userId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _authenticationService.ActivateUserAsync(userId, cancellationToken);
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
                _logger.LogError(ex, "Activate-User - An Unexpected error occurred while activate user.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("activate user."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves a list of user accounts filtered by their activation status.
        /// </summary>
        [HttpGet("all-by-activation-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllByActivationStatus([FromQuery] PaginationRequestDto pagination, bool isActive, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _userService.GetUsersBasedOnActivationStatus(pagination, cancellationToken, isActive);
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
                _logger.LogError(ex, "GetAllByActivationStatus - An Unexpected error occurred while activate user.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" get all users."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves a complete list of all user accounts, regardless of activation status.
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _userService.GetAllUsersAsync(pagination, cancellationToken);
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
                _logger.LogError(ex, "GetAll - An Unexpected error occurred while activate user.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" get all users."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves detailed information for a specific user account by its unique identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the user whose data is to be retrieved.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation if needed.</param>
        /// <returns>
        /// Returns 200 OK with the user's data if found.
        /// Returns 404 Not Found if the user does not exist.
        /// Returns 500 Internal Server Error if an unexpected error occurs during data retrieval.
        /// </returns>
        [HttpGet("get-one/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserByIdAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _userService.FindUserAsync(cancellationToken, null, null, id, true, true);
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
                _logger.LogError(ex, "GetUserById - An Unexpected error occurred while activate user.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" get one user by id."), Data = null });
            }
        }
    }
}
