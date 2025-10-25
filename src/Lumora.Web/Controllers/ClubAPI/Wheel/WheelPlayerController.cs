using Lumora.DataAnnotations;
using Lumora.Interfaces.ClubIntf;
using static Lumora.Infrastructure.PermissionInfra.Permissions;

namespace Up.Controllers.FortuneWheelAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.WheelRoles)]
    public class WheelPlayerController(
        ILogger<WheelPlayerController> logger,
        IWheelPlayerService wheelPlayerService) : AuthenticatedController
    {
        private readonly ILogger<WheelPlayerController> _logger = logger;
        private readonly IWheelPlayerService _wheelPlayerService = wheelPlayerService;

        /// <summary>
        /// Spins the wheel for the current authenticated user for a specified award.
        /// </summary>
        /// <param name="awardId">The identifier of the WheelAward to be spun.</param>
        /// <returns>
        /// Returns the spin result if successful; otherwise, returns HTTP 400 for validation or business errors such as ineligible spin attempts.
        /// </returns>
        [HttpPost("spin")]
        [RequiredPermission(WheelPlayerPermissions.Spin)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Spin([FromQuery] int awardId)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.SpinAsync(CurrentUserId!, awardId);
                if (result.IsSuccess == false) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return this.UnexpectedError("spinning wheel");
            }
        }

        /// <summary>
        /// Checks whether a player is eligible to spin the wheel on the current day.
        /// </summary>
        [HttpGet("can-play")]
        [AllowAnonymous]
        [RequiredPermission(WheelPlayerPermissions.CanPlay)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CanPlay()
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.CanPlayTodayAsync(CurrentUserId!);
                if (result.IsSuccess == false) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return this.UnexpectedError("checking if player can play");
            }
        }

        /// <summary>
        /// Retrieves the player's spin result for the current day, if available.
        /// </summary>
        [HttpGet("today-spins")]
        [RequiredPermission(WheelPlayerPermissions.TodaySpin)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTodaySpins()
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.GetTodaySpinAsync(CurrentUserId!);
                if (result.IsSuccess == false) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return this.UnexpectedError("getting today spin");
            }
        }

        /// <summary>
        /// Retrieves the complete spin history of a specific player.
        /// </summary>
        [HttpGet("history")]
        [RequiredPermission(WheelPlayerPermissions.History)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.GetPlayerHistoryAsync(CurrentUserId!);
                if (result.IsSuccess == false) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return this.UnexpectedError("getting player history");
            }
        }
    }
}

