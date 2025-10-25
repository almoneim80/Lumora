namespace Lumora.Web.Controllers.Authentication
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    public class AuthenticationController(
        IMapper mapper,
        UserManager<User> userManager,
        ILogger<AuthenticationController> logger,
        IAuthenticationService authenticationService,
        AuthenticationMessage messages) : AuthenticatedController
    {
        protected readonly IMapper _mapper = mapper;
        protected readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<AuthenticationController> _logger = logger;
        private readonly IAuthenticationService _authenticationService = authenticationService;

        /// <summary>
        /// Creates a new user account using the provided registration information.
        /// </summary>
        /// <param name="dto">The registration data including personal information, phone number, and password.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns a 200 OK with a <see cref="GeneralResult"/> if registration is successful.
        /// Returns 400 Bad Request or 422 Unprocessable Entity if input data is invalid or registration fails.
        /// Returns 500 Internal Server Error if an unexpected error occurs during the process.
        /// </returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _authenticationService.RegisterAsync(dto, cancellationToken);
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
                _logger.LogError(ex, "Register - An Unexpected error occurred while creating user.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("Creating user"), Data = null });
            }
        }

        /// <summary>
        /// Authenticates a user using their phone number and password.
        /// </summary>
        /// <param name="dto">The login credentials including phone number and password.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> containing the authentication token if login succeeds.
        /// Returns 401 Unauthorized if credentials are incorrect.
        /// Returns 500 Internal Server Error if an unexpected error occurs during the authentication process.
        /// </returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Login API", Description = "Authenticates a user using a phone number and password, and issues an access token upon successful login.",
            Tags = new[] { "Authentication" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _authenticationService.LoginAsync(dto, cancellationToken);
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
                _logger.LogError(ex, "Login - Unexpected error occurred while logging in.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("Login"), Data = null });
            }
        }

        /// <summary>
        /// Issues a new access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token previously issued during login.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> containing the new token if the refresh is successful.
        /// Returns 400 Bad Request if the refresh token is invalid or expired.
        /// Returns 500 Internal Server Error if an unexpected error occurs during token renewal.
        /// </returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authenticationService.RefreshTokenAsync(refreshToken, cancellationToken);
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
                _logger.LogError(ex, "RefreshToken - An Unexpected error occurred while refreshing token.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("Refreshing token"), Data = null });
            }
        }

        /// <summary>
        /// Logs out the currently authenticated user and invalidates the associated refresh token if provided.
        /// </summary>
        /// <param name="refreshToken">Optional. The refresh token to be invalidated during logout.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if logout is successful.
        /// Returns appropriate error responses if logout fails due to token issues or unexpected errors.
        /// </returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout([FromQuery] string refreshToken, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _authenticationService.LogoutAsync(CurrentUserId!, refreshToken, cancellationToken);
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
                _logger.LogError(ex, "Logout - An Unexpected error occurred while loged user out.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("logout"), Data = null });
            }
        }
    }
}
