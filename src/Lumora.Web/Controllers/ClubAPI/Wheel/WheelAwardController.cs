using Lumora.DataAnnotations;
using Lumora.DTOs.Club;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ClubIntf;
namespace Lumora.Controllers.ClubAPI.Wheel
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.WheelRoles)]
    public class WheelAwardController(
        WheelMessag messages,
        IWheelAwardService wheelAwardService,
        ILogger<WheelAwardController> logger) : AuthenticatedController
    {
        /// <summary>
        /// Retrieves all wheel awards with pagination.
        /// </summary>
        [HttpGet("get-all")]
        [RequiredPermission(Permissions.WheelAwardPermissions.GetAll)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<WheelAwardDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await wheelAwardService.GetAllAsync(pagination, cancellationToken);
                return result.IsSuccess ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving all wheel awards.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("get all wheel awards"), Data = null });
            }
        }
    }
}

