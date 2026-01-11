namespace Lumora.Web.Controllers.LocalizationAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    public class LocalizationController(ILocalizationManager localization, GeneralMessage messages) : ControllerBase
    {
        private readonly ILocalizationManager _localization = localization;
        private readonly GeneralMessage _messages = messages;

        /// <summary>
        /// Sets the preferred UI culture for the current user session and stores it in a secure cookie.
        /// </summary>
        /// <param name="culture">The culture code (e.g., "en-US", "fr-FR") to be applied to the current user context.</param>
        /// <returns>
        /// Returns 200 OK if the culture was successfully set; otherwise, returns 400 Bad Request if the provided culture is invalid.
        /// </returns>
        [HttpPost("setculture")]
        public IActionResult SetCulture([FromForm] string culture)
        {
            try
            {
                _localization.SetCulture(culture);

                // Store the preferred culture in a cookie
                Response.Cookies.Append("PreferredCulture", culture, new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    HttpOnly = true,
                    Secure = true, // Use this if your site uses HTTPS
                });

                return Ok($"Culture set to {culture}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of supported UI cultures available for localization.
        /// </summary>
        /// <returns>A collection of <see cref="CultureDto"/> representing the supported cultures.</returns>
        [HttpGet("Cultures")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CultureDto>> GetCultures()
        {
            return Ok(_localization.GetCultures());
        }

        /// <summary>
        /// Retrieves a list of countries supported by the localization system.
        /// </summary>
        /// <returns>A collection of <see cref="CountryInfo"/> containing metadata about each supported country.</returns>
        [HttpGet("Countries")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CountryInfo>> GetCountries()
        {
            return Ok(_localization.GetCountries());
        }

        /// <summary>
        /// Retrieves a list of available parental ratings used in the system.
        /// </summary>
        /// <returns>A collection of <see cref="ParentalRating"/> representing parental control levels.</returns>
        [HttpGet("ParentalRatings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ParentalRating>> GetParentalRatings()
        {
            return Ok(_localization.GetParentalRatings());
        }

        /// <summary>
        /// Retrieves localization configuration options available in the application.
        /// </summary>
        /// <returns>A collection of <see cref="LocalizationOption"/> detailing available localization options.</returns>
        [HttpGet("Options")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<LocalizationOption>> GetLocalizationOptions()
        {
            return Ok(_localization.GetLocalizationOptions());
        }

        /// <summary>
        /// Retrieves a predefined success or welcome message for the client.
        /// </summary>
        /// <returns>An object containing a welcome message string.</returns>
        [HttpGet("success")]
        public IActionResult GetSuccessMessage()
        {
            var message = _messages.MsgWelcome;
            return Ok(new { Message = message });
        }
    }
}
