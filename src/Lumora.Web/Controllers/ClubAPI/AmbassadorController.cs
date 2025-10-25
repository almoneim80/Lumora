using Lumora.DataAnnotations;
using Lumora.DTOs.Club;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ClubIntf;

namespace Lumora.Web.Controllers.ClubAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.PostRoles)]
    public class AmbassadorController(
    IAmbassadorService ambassadorService,
    ClubMessage messages,
    ILogger<AmbassadorController> logger) : AuthenticatedController
    {
        private readonly IAmbassadorService _ambassadorService = ambassadorService;
        private readonly ClubMessage _messages = messages;
        private readonly ILogger<AmbassadorController> _logger = logger;

        /// <summary>
        /// Retrieves the current active ambassador.
        /// </summary>
        [HttpGet("current")]
        [RequiredPermission(Permissions.AmbassadorPermissions.GetCurrent)]
        [ProducesResponseType(typeof(GeneralResult<AmbassadorDetailsDto?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<AmbassadorDetailsDto?>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
        {
            try
            {
                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;
               
                var result = await _ambassadorService.GetCurrentAmbassadorAsync(cancellationToken);
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
                _logger.LogError(ex, "AmbassadorController - GetCurrent: Unexpected error.");
                return StatusCode(500, new GeneralResult<AmbassadorDetailsDto?>(false, _messages.GetUnexpectedErrorMessage("Get current ambassador"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves ambassador assignment history.
        /// </summary>
        [HttpGet("history")]
        [RequiredPermission(Permissions.AmbassadorPermissions.GetHistory)]
        [ProducesResponseType(typeof(GeneralResult<List<AmbassadorDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<List<AmbassadorDetailsDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
        {
            try
            {
                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _ambassadorService.GetAmbassadorHistoryAsync(cancellationToken);
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
                _logger.LogError(ex, "AmbassadorController - GetHistory: Unexpected error.");
                return StatusCode(500, new GeneralResult<List<AmbassadorDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get ambassador history"), null, ErrorType.InternalServerError));
            }
        }
    }
}
