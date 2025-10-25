namespace Lumora.Web.Controllers.Authentication
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    public class PasswordManagementController(
        IMapper mapper,
        UserManager<User> userManager,
        ILogger<PasswordManagementController> logger,
        IAuthenticationService authenticationService,
        GeneralMessage messages) : AuthenticatedController
    {
        protected readonly IMapper _mapper = mapper;
        protected readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<PasswordManagementController> _logger = logger;
        private readonly IAuthenticationService _authenticationService = authenticationService;

        /// <summary>
        /// Changes the current password of the authenticated user.
        /// </summary>
        /// <param name="dto">An object containing the current password and the new desired password.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the password was changed successfully.
        /// Returns 400 Bad Request if the current password is incorrect or the input is invalid.
        /// Returns 404 Not Found if the user does not exist.
        /// Returns 500 Internal Server Error if an unexpected error occurs during the password change process.
        /// </returns>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                dto.UserId = CurrentUserId;
                var result = await _authenticationService.ChangePasswordAsync(dto, cancellationToken);
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
                _logger.LogError(ex, "Change-Password - An Unexpected error occurred while change password.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("change password."), Data = null });
            }
        }

        /// <summary>
        /// Initiates the password reset process by sending a reset token to the user's registered email or phone number.
        /// </summary>
        /// <param name="dto">An object containing user identification data such as email or phone number.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the reset process was initiated successfully.
        /// Returns 400 Bad Request if the input data is invalid or user is not eligible for password reset.
        /// Returns 404 Not Found if the user cannot be found based on the provided data.
        /// Returns 500 Internal Server Error if an unexpected error occurs during the process.
        /// </returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _authenticationService.ForgotPasswordAsync(dto, cancellationToken);
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
                _logger.LogError(ex, "Forgot-Password - An Unexpected error occurred while forgot password.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("forgot password."), Data = null });
            }
        }

        /// <summary>
        /// Completes the password reset process using a valid reset token and sets a new password for the user.
        /// </summary>
        /// <param name="dto">An object containing the reset token, user identifier, and the new password.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the password was reset successfully.
        /// Returns 400 Bad Request if the token is invalid, expired, or input data is malformed.
        /// Returns 404 Not Found if the user does not exist or token does not match any record.
        /// Returns 500 Internal Server Error if an unexpected error occurs during the reset process.
        /// </returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _authenticationService.ResetPasswordAsync(dto, cancellationToken);
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
                _logger.LogError(ex, "Reset-Password - An Unexpected error occurred while reset password.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("reset password."), Data = null });
            }
        }
    }
}
