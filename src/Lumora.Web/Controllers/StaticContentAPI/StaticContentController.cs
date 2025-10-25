using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Lumora.DataAnnotations;
using Lumora.DTOs.StaticContent;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.StaticContentIntf;

namespace Lumora.Web.Controllers.StaticContentAPI
{
    [AllowAnonymous]
    [ApiController]
    [Route("wejha/api/[controller]")]
    public class StaticContentController(
        IStaticContentService staticContentService,
        ILocalizationManager localization,
        FileValidatorHelper fileValidator,
        IExtendedBaseService extendedBaseService,
        ILogger<StaticContentController> logger) : AuthenticatedController
    {
        private readonly IStaticContentService _service = staticContentService;
        private readonly ILocalizationManager _localization = localization;
        private readonly IExtendedBaseService _extendedBaseService = extendedBaseService;
        private readonly ILogger<StaticContentController> _logger = logger;
        private readonly FileValidatorHelper _fileValidator = fileValidator;

        /// <summary>
        /// Get all static content entries.
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<StaticContent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<StaticContent>>> GetAll(
            [FromQuery] string? group = null,
            [FromQuery] string? language = null,
            [FromQuery] bool? isActive = true)
        {
            var result = await _service.GetAllAsync(group, language, isActive);
            return Ok(result);
        }

        /// <summary>
        /// Get a content entry by key and language.
        /// </summary>
        [HttpGet("item")]
        [ProducesResponseType(typeof(StaticContent), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StaticContent?>> Get([FromQuery] string key, [FromQuery] string language = "ar")
        {
            var result = await _service.GetAsync(key, language);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Get value of a content entry only (used in UI).
        /// </summary>
        [HttpGet("value")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string?>> GetValue([FromQuery] string key, [FromQuery] string language = "ar")
        {
            var value = await _service.GetValueAsync(key, language);
            if (value == null) return NotFound();
            return Ok(value);
        }

        /// <summary>
        /// Set or update a static content value only (Upsert).
        /// </summary>
        [HttpPut("value")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [RequiredPermission(Permissions.StaticContentPermissions.Edit)]
        public async Task<IActionResult> Update([FromQuery] string key, [FromQuery] string value, [FromQuery] string language = "ar")
        {
            var userCheck = CheckUserOrUnauthorized();
            if (userCheck != null) return userCheck;

            var result = await _service.SetValueAsync(key, value, language);
            return result.IsSuccess == true ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Create or update a full static content entry.
        /// </summary>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [RequiredPermission(Permissions.StaticContentPermissions.Create)]
        public async Task<IActionResult> Create([FromForm] StaticContentCreateFormDto content)
        {
            var userCheck = CheckUserOrUnauthorized();
            if (userCheck != null) return userCheck;

            string? mediaFileName = null;

            if (content.MediaFile is not null)
            {
                var fileResult = content.MediaFile.PrepareValidatedFile(
                    content.MediaType == StaticContentMediaType.Image ? Enums.MediaType.Image : Enums.MediaType.Video, _fileValidator);

                if (!fileResult.IsValid)
                {
                    _logger.LogWarning("StaticContent Create: Media file validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                    return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                }

                mediaFileName = fileResult.UniqueName;

                // TODO: Upload file
                // await _fileStorage.UploadAsync(fileResult.Stream, mediaFileName, "static-content");
            }

            var createDto = new StaticContentCreateDto
            {
                Key = content.Key,
                Value = content.Value,
                Language = content.Language,
                Group = content.Group,
                ContentType = content.ContentType,
                MediaType = content.MediaType,
                MediaUrl = mediaFileName,
                MediaAlt = content.MediaAlt,
                Note = content.Note,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = await _service.SaveAsync(createDto);
            return result.IsSuccess == true ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Soft delete a content entry.
        /// </summary>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [RequiredPermission(Permissions.StaticContentPermissions.Delete)]
        public async Task<IActionResult> Delete([FromQuery] string key, [FromQuery] string language = "ar")
        {
            var userCheck = CheckUserOrUnauthorized();
            if (userCheck != null) return userCheck;

            var result = await _service.DeleteAsync(key, language);
            return result.IsSuccess == true ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get static content media types.
        /// </summary>
        [HttpGet("media-type")]
        [ProducesResponseType(typeof(IEnumerable<EnumData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        [RequiredPermission(Permissions.StaticContentPermissions.GetStaticContentMediaType)]
        public ActionResult<IEnumerable<EnumData>> GetStaticContentMediaType()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new GeneralResult { IsSuccess = false, Message = _localization.GetLocalizedString("UserNotLoggedIn") });
                }

                var enumValues = _extendedBaseService.GetEnumValues<StaticContentMediaType>();
                return Ok(enumValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the static content GetStaticContentMediaType method.");
                return this.UnexpectedError("getting static content media types.");
            }
        }

        /// <summary>
        /// Get static content types.
        /// </summary>
        [HttpGet("content-type")]
        [ProducesResponseType(typeof(IEnumerable<EnumData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        [RequiredPermission(Permissions.StaticContentPermissions.GetStaticContentType)]
        public ActionResult<IEnumerable<EnumData>> GetStaticContentType()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new GeneralResult { IsSuccess = false, Message = _localization.GetLocalizedString("UserNotLoggedIn") });
                }

                var enumValues = _extendedBaseService.GetEnumValues<StaticContentType>();
                return Ok(enumValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the static content GetStaticContentType method.");
                return this.UnexpectedError("getting static content types.");
            }
        }
    }
}
