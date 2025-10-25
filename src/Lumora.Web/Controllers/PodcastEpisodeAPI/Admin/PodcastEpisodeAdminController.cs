using Microsoft.AspNetCore.Identity;
using Lumora.DataAnnotations;
using Lumora.DTOs.Podcast;
using Lumora.Interfaces.PodcastEpisodeIntf;
using static Lumora.Infrastructure.PermissionInfra.Permissions;

namespace Lumora.Web.Controllers.PodcastEpisodeAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class PodcastEpisodeAdminController(
            IPodcastEpisodeService podcastEpisodeService,
            PodcastEpisodeMessage messages,
            FileValidatorHelper fileValidator,
            ILogger<PodcastEpisodeController> logger) : AuthenticatedController
    {
        private readonly IPodcastEpisodeService _podcastEpisodeService = podcastEpisodeService;
        private readonly ILogger<PodcastEpisodeController> _logger = logger;
        private readonly PodcastEpisodeMessage _messages = messages;
        private readonly FileValidatorHelper _fileValidator = fileValidator;

        /// <summary>
        /// Creates a new podcast episode.
        /// </summary>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(PodcastEpisodeAdminPermissions.Create)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] PodcastEpisodeCreateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (formDto.Thumbnail == null || formDto.Thumbnail.Length == 0)
                {
                    _logger.LogWarning("PodcastEpisodeController - Create: No thumbnail file uploaded.");
                    return BadRequest(new GeneralResult(false, _messages.MsgFileRequired, null, ErrorType.Validation));
                }

                var fileResult = formDto.Thumbnail.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                if (!fileResult.IsValid)
                {
                    _logger.LogWarning("PodcastEpisodeController - Create: Thumbnail validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                    return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                }

                var dto = new PodcastEpisodeCreateDto
                {
                    Title = formDto.Title?.Trim(),
                    Description = formDto.Description?.Trim(),
                    EpisodeNumber = formDto.EpisodeNumber,
                    YoutubeUrl = formDto.YoutubeUrl?.Trim(),
                    ThumbnailUrl = fileResult.UniqueName
                };

                // TODO: Upload thumbnail file
                // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "podcast-thumbnails");

                var result = await _podcastEpisodeService.CreateAsync(dto, cancellationToken);
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
                _logger.LogError(ex, "PodcastEpisodeController - Create: Unexpected error.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("creating podcast episode"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Updates an existing podcast episode by ID.
        /// </summary>
        [HttpPatch("update/{id:int}")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(PodcastEpisodeAdminPermissions.Update)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromForm] PodcastEpisodeUpdateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                string? thumbnailUrl = null;

                if (formDto.ThumbnailFile != null)
                {
                    var fileResult = formDto.ThumbnailFile.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                    if (!fileResult.IsValid)
                    {
                        _logger.LogWarning("PodcastEpisodeController - Update: Thumbnail file validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                    }

                    thumbnailUrl = fileResult.UniqueName;

                    // TODO: Upload the file
                    // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "podcast-thumbnails");
                }

                var dto = new PodcastEpisodeUpdateDto
                {
                    Title = formDto.Title,
                    Description = formDto.Description,
                    YoutubeUrl = formDto.YoutubeUrl,
                    EpisodeNumber = formDto.EpisodeNumber,
                    ThumbnailUrl = thumbnailUrl
                };

                var result = await _podcastEpisodeService.UpdateAsync(id, dto, cancellationToken);
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
                _logger.LogError(ex, "PodcastEpisodeController - Update: Unexpected error while updating podcast episode.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating podcast episode"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Soft deletes a podcast episode by ID.
        /// </summary>
        [HttpDelete("delete/{id:int}")]
        [RequiredPermission(PodcastEpisodeAdminPermissions.Delete)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _podcastEpisodeService.DeleteAsync(id, cancellationToken);
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
                _logger.LogError(ex, "PodcastEpisodeController - Delete: Unexpected error while deleting podcast episode.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("deleting podcast episode"), null, ErrorType.InternalServerError));
            }
        }
    }
}
