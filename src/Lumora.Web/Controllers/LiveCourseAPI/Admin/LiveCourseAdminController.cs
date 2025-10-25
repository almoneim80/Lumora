using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Lumora.DataAnnotations;
using Lumora.DTOs.LiveCourse;
using Lumora.DTOs.Test;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.LiveCourseIntf;
using Xabe.FFmpeg;

namespace Lumora.Web.Controllers.LiveCourseAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class LiveCourseAdminController(
    ILogger<LiveCourseAdminController> logger,
    LiveCourseMessage messages,
    FileValidatorHelper fileValidator,
    ILiveCourseService liveCourseService) : AuthenticatedController
    {
        private readonly ILogger<LiveCourseAdminController> _logger = logger;
        private readonly LiveCourseMessage _messages = messages;
        private readonly ILiveCourseService _liveCourseService = liveCourseService;
        private readonly FileValidatorHelper _fileValidator = fileValidator;

        /// <summary>
        /// Creates a new live course.
        /// </summary>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.LiveCourseAdminPermissions.Create)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] LiveCourseCreateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (formDto == null)
                {
                    _logger.LogWarning("CreateLiveCourse: Deserialization failed, dto is null.");
                    return BadRequest(new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.InvalidData));
                }

                string? courseImageName = null;
                if (formDto.CourseImage is not null)
                {
                    var fileResult = formDto.CourseImage.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                    if (!fileResult.IsValid)
                    {
                        _logger.LogWarning("CreateLiveCourse: Course image validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                    }

                    courseImageName = fileResult.UniqueName;

                    // TODO: Upload file
                    // await _fileStorage.UploadAsync(fileResult.Stream, dto.ImagePath, "live-course-images");
                }

                var dto = new LiveCourseCreateDto
                {
                    Title = formDto.Title,
                    Price = formDto.Price,
                    Description = formDto.Description,
                    ImagePath = courseImageName,
                    StudyWay = formDto.StudyWay,
                    StartDate = formDto.StartDate ?? DateTimeOffset.UtcNow,
                    EndDate = formDto.EndDate,
                    Link = formDto.Link,
                    Lecturer = formDto.Lecturer,
                };

                var result = await _liveCourseService.CreateAsync(dto, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseController - Create: Unexpected error while creating live course.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Create live course"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Updates an existing live course.
        /// </summary>
        [HttpPatch("update")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.LiveCourseAdminPermissions.Update)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromQuery] int courseId, [FromForm] LiveCourseUpdateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                string? courseImageName = null;
                if (formDto.ImageFile is not null)
                {
                    var fileResult = formDto.ImageFile.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                    if (!fileResult.IsValid)
                    {
                        _logger.LogWarning("UpdateLiveCourse: Image validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                    }

                    courseImageName = fileResult.UniqueName;

                    // TODO: Upload file
                    // await _fileStorage.UploadAsync(fileResult.Stream, dto.ImagePath, "live-course-images");
                }

                var dto = new LiveCourseUpdateDto
                {
                    Title = formDto.Title,
                    Price = formDto.Price,
                    Description = formDto.Description,
                    ImagePath = courseImageName,
                    StudyWay = formDto.StudyWay,
                    StartDate = formDto.StartDate,
                    EndDate = formDto.EndDate,
                    Link = formDto.Link,
                    Lecturer = formDto.Lecturer,
                };

                var result = await _liveCourseService.UpdateAsync(courseId, dto, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseController - Update: Unexpected error while updating course ID {CourseId}.", courseId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Update course"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Deletes a live course by its ID.
        /// </summary>
        [HttpDelete("delete")]
        [RequiredPermission(Permissions.LiveCourseAdminPermissions.Delete)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _liveCourseService.DeleteAsync(courseId, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseController - Delete: Unexpected error while deleting course ID {CourseId}.", courseId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Delete course"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves paginated list of users subscribed to a specific live course.
        /// </summary>
        [HttpGet("subscribers")]
        [RequiredPermission(Permissions.LiveCourseAdminPermissions.GetSubscribers)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<UserLiveCourseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseSubscribers([FromQuery] int courseId, [FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _liveCourseService.GetCourseSubscribersAsync(courseId, pagination, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.BadRequest => BadRequest(result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseController - GetCourseSubscribers: Unexpected error while retrieving subscribers for course ID {CourseId}.", courseId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("fetching course subscribers"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Changes the active status of a live course.
        /// </summary>
        [HttpPatch("status")]
        [RequiredPermission(Permissions.LiveCourseAdminPermissions.SetStatus)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetStatus([FromQuery] int courseId, [FromQuery] bool isActive, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _liveCourseService.SetActiveStatusAsync(courseId, isActive, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseController - SetStatus: Unexpected error while updating status for course ID {CourseId}.", courseId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("updating course status"),
                    Data = null
                });
            }
        }
    }
}
