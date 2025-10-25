namespace Lumora.Web.Controllers.Authentication
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    public class ConfirmationController(
        IMapper mapper,
        UserManager<User> userManager,
        ILogger<ConfirmationController> logger,
        IAuthenticationService authenticationService,
        GeneralMessage messages) : AuthenticatedController
    {
        protected readonly IMapper _mapper = mapper;
        protected readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<ConfirmationController> _logger = logger;
        private readonly IAuthenticationService _authenticationService = authenticationService;

        /// <summary>
        /// Confirms the user's email address using a verification token.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose email is to be confirmed.</param>
        /// <param name="token">The verification token sent to the user's email address.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the email confirmation is successful.
        /// Returns 400 Bad Request if the token is invalid or malformed.
        /// Returns 404 Not Found if the user does not exist.
        /// Returns 500 Internal Server Error if an unexpected error occurs during the confirmation process.
        /// </returns>
        [HttpPost("confirm-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authenticationService.ConfirmEmailAsync(userId, token, cancellationToken);
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
                _logger.LogError(ex, "Confirm-Email - An Unexpected error occurred while confirm email.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("confirm email."), Data = null });
            }
        }

        /// <summary>
        /// Confirms the user's phone number using a verification code.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose phone number is to be confirmed.</param>
        /// <param name="code">The verification code sent to the user's phone number.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the phone confirmation is successful.
        /// Returns 400 Bad Request if the code is invalid or expired.
        /// Returns 404 Not Found if the user does not exist.
        /// Returns 500 Internal Server Error if an unexpected error occurs during the confirmation process.
        /// </returns>
        [HttpPost("confirm-phone")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmPhone([FromQuery] string userId, [FromQuery] string code, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authenticationService.ConfirmPhoneAsync(userId, code, cancellationToken);
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
                _logger.LogError(ex, "Confirm-Phone - An Unexpected error occurred while confirm phone.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("confirm phone."), Data = null });
            }
        }

        /// <summary>
        /// Resends the confirmation email containing the verification token to the specified email address.
        /// </summary>
        /// <param name="email">The email address to which the confirmation message should be resent.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the email is sent successfully.
        /// Returns 400 Bad Request if the email format is invalid or user is already confirmed.
        /// Returns 404 Not Found if no user is associated with the specified email.
        /// Returns 500 Internal Server Error if an unexpected error occurs while sending the email.
        /// </returns>
        [HttpPost("resend-confirmation/email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string email, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authenticationService.ResendConfirmationEmailAsync(email, cancellationToken);
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
                _logger.LogError(ex, "Resend-Confirmation-Email - An Unexpected error occurred while resend confirmation email.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("resend confirmation email."), Data = null });
            }
        }

        /// <summary>
        /// Resends the confirmation SMS containing the verification code to the specified phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to which the confirmation SMS should be resent.</param>
        /// <param name="cancellationToken">Token for cancelling the operation if requested.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GeneralResult"/> if the SMS is sent successfully.
        /// Returns 400 Bad Request if the phone number format is invalid or user is already confirmed.
        /// Returns 404 Not Found if no user is associated with the specified phone number.
        /// Returns 500 Internal Server Error if an unexpected error occurs while sending the SMS.
        /// </returns>
        [HttpPost("resend-confirmation/sms")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResendConfirmationSms([FromQuery] string phoneNumber, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authenticationService.ResendConfirmationSmsAsync(phoneNumber, cancellationToken);
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
                _logger.LogError(ex, "Resend-Confirmation-Sms - An Unexpected error occurred while resend confirmation sms.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("resend confirmation sms."), Data = null });
            }
        }
    }
}
