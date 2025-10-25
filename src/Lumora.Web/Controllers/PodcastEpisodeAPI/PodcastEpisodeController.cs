using Lumora.DataAnnotations;
using Lumora.DTOs.Podcast;
using Lumora.Interfaces.PodcastEpisodeIntf;
using static Lumora.Infrastructure.PermissionInfra.Permissions;
namespace Lumora.Web.Controllers.PodcastEpisodeAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.PodcastRoles)]
    public class PodcastEpisodeController(
        IPodcastEpisodeService podcastEpisodeService,
        PodcastEpisodeMessage messages,
        ILogger<PodcastEpisodeController> logger) : AuthenticatedController
    {
        private readonly IPodcastEpisodeService _podcastEpisodeService = podcastEpisodeService;
        private readonly ILogger<PodcastEpisodeController> _logger = logger;
        private readonly PodcastEpisodeMessage _messages = messages;

        /// <summary>
        /// Retrieves details of a specific podcast episode by ID.
        /// </summary>
        /// <param name="id">The ID of the podcast episode.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("get-by-id/{id:int}")]
        [RequiredPermission(PodcastEpisodePermissions.GetById)]
        [ProducesResponseType(typeof(GeneralResult<PodcastEpisodeDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<PodcastEpisodeDetailsDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult<PodcastEpisodeDetailsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<PodcastEpisodeDetailsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _podcastEpisodeService.GetByIdAsync(id, cancellationToken);
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
                _logger.LogError(ex, "PodcastEpisodeController - GetById: Unexpected error while retrieving podcast episode. Id={Id}", id);
                return StatusCode(500,
                    new GeneralResult<PodcastEpisodeDetailsDto>(false, _messages.GetUnexpectedErrorMessage("retrieving podcast episode"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves all podcast episodes with pagination.
        /// </summary>
        /// <param name="pagination">Pagination parameters (page number, size).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("get-all")]
        [RequiredPermission(PodcastEpisodePermissions.GetAll)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _podcastEpisodeService.GetAllAsync(pagination, cancellationToken);
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
                _logger.LogError(ex, "PodcastEpisodeController - GetAll: Unexpected error while retrieving podcast episodes.");
                return StatusCode(500,
                    new GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>(
                        false, _messages.GetUnexpectedErrorMessage("retrieving podcast episodes"), null, ErrorType.InternalServerError));
            }
        }
    }
}
