using Lumora.DataAnnotations;
using Lumora.DTOs.Club;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ClubIntf;
namespace Lumora.Web.Controllers.ClubAPI.Admin
{
    public class AdminAmbassadorController(
    IAmbassadorService ambassadorService,
    ClubMessage messages,
    ILogger<AdminAmbassadorController> logger) : AuthenticatedController
    {
        private readonly IAmbassadorService _ambassadorService = ambassadorService;
        private readonly ClubMessage _messages = messages;
        private readonly ILogger<AdminAmbassadorController> _logger = logger;

        /// <summary>
        /// Assigns a user as an ambassador for a specific duration.
        /// </summary>
        [HttpPost("assign")]
        [RequiredPermission(Permissions.AdminAmbassadorPermissions.Assign)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Assign([FromBody] AmbassadorAssignDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _ambassadorService.AssignAmbassadorAsync(dto, CurrentUserId!, cancellationToken);
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
                _logger.LogError(ex, "AmbassadorController - Assign: Unexpected error.");
                return StatusCode(500, new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("Assign ambassador"), 0, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Removes an ambassador assignment.
        /// </summary>
        [HttpDelete("remove")]
        [RequiredPermission(Permissions.AdminAmbassadorPermissions.Remove)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Remove([FromQuery] int id, CancellationToken cancellationToken)
        {
            try
            {
                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _ambassadorService.RemoveAmbassadorAsync(id, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorController - Remove: Unexpected error.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Remove ambassador"), null, ErrorType.InternalServerError));
            }
        }
    }
}
