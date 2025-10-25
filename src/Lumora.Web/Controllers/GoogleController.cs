//namespace Lumora.Web.Controllers
//{
//    public class GoogleController : ControllerBase
//    {
//        private readonly ILogger<GoogleController> _logger;
//        private readonly ILocalizationManager? _localization;
//        private readonly IExternalAuthService _externalAuthService;

//        public GoogleController(
//            ILogger<GoogleController> logger,
//            ILocalizationManager? localization,
//            IExternalAuthService externalAuthService)
//        {
//            _localization = localization;
//            _logger = logger;
//            _externalAuthService = externalAuthService;
//        }

//        /// <summary>
//        /// Initiates the Google login process by generating a login URL.
//        /// </summary>
//        [HttpGet("google/login")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//        public IActionResult GoogleLogin()
//        {
//            try
//            {
//                string loginUrl = _externalAuthService.GenerateGoogleLoginUrl(new List<string> { "email", "profile" });
//                _logger.LogInformation("Google login URL generated successfully.");
//                return Redirect(loginUrl);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error initiating Google login.");
//                return StatusCode(500, _localization!.GetLocalizedString("GoogleLoginUrlError"));
//            }
//        }

//        /// <summary>
//        /// Callback endpoint for Google after the user has authenticated.
//        /// </summary>
//        [HttpGet("callback")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> GoogleCallback([FromQuery] string code)
//        {
//            if (string.IsNullOrEmpty(code))
//            {
//                _logger.LogWarning("Google callback called with empty code.");
//                return BadRequest(_localization!.GetLocalizedString("AuthorizationCodeMissing"));
//            }

//            try
//            {
//                var authResult = await _externalAuthService.HandleGoogleAuthCallbackAsync(code);
//                if (authResult.Success)
//                {
//                    _logger.LogInformation("Google callback handled successfully for user {Email}.", authResult.Email);
//                    return Ok(new
//                    {
//                        Message = authResult.ErrorMessage,
//                        Token = authResult.Token,
//                        Email = authResult.Email
//                    });
//                }
//                else
//                {
//                    _logger.LogWarning("Google callback failed with error: {ErrorMessage}", authResult.ErrorMessage);
//                    return StatusCode(500, authResult.ErrorMessage);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error handling Google callback.");
//                return StatusCode(500, _localization!.GetLocalizedString("GoogleAuthError"));
//            }
//        }
//    }
//}
