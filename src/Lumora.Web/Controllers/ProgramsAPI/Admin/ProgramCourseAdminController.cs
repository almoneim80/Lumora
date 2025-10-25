using System.Text.Json;
using Up.Extensions;
using Lumora.DataAnnotations;
using Lumora.DTOs.Test;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;
using Xabe.FFmpeg;

namespace Lumora.Web.Controllers.ProgramsAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class ProgramCourseAdminController(
            CourseMessage messages,
            IExtendedBaseService extendedBaseService,
            IProgramCourseService courseService,
            ICascadeDeleteService deleteService,
            FileValidatorHelper fileValidator,
            ILogger<ProgramCourseController> logger) : AuthenticatedController
    {
        private readonly ICascadeDeleteService _deleteService = deleteService;
        private readonly IExtendedBaseService _extendedBaseService = extendedBaseService;
        private readonly ILogger<ProgramCourseController> _logger = logger;
        private readonly FileValidatorHelper _fileValidator = fileValidator;

        /// <summary>
        /// Creates a new program course along with its associated lessons and content.
        /// </summary>
        /// <param name="formDto">Data transfer object containing the course details and its lessons to be created.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests during the asynchronous operation.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// - 200 OK if the course creation succeeds.
        /// - 400 Bad Request if the input data is invalid.
        /// - 401 Unauthorized if the user is not authorized.
        /// - 404 Not Found if a required resource is missing.
        /// - 500 Internal Server Error for unexpected failures.
        /// </returns>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.ProgramCourseAdminPermissions.Create)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] CourseWithLessonsCreateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                CourseWithLessonsCreateDto? dto;
                try
                {
                    dto = JsonSerializer.Deserialize<CourseWithLessonsCreateDto>(formDto.CourseJson!, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (dto == null)
                    {
                        _logger.LogWarning("CreateCourse: Deserialization failed, dto is null.");
                        return BadRequest(new GeneralResult(false, messages.MsgDtoNull, null, ErrorType.InvalidData));
                    }

                    // # course logo -*-  -*-  -*- 
                    if (formDto.CourseLogo is not null)
                    {
                        var fileResult = formDto.CourseLogo.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                        if (!fileResult.IsValid)
                        {
                            _logger.LogWarning("CreateCourse: Course logo validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                            return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                        }

                        dto.Logo = fileResult.UniqueName;

                        // TODO: Implement file upload
                        // await _fileStorage.UploadAsync(fileResult.Stream, dto.Logo, "courses");
                    }

                    // # lessons video -*-  -*-  -*-
                    if (dto.Lessons is not null)
                    {
                        for (int i = 0; i < dto.Lessons.Count; i++)
                        {
                            var formFile = Request.Form.Files[$"lessonVideo_{i}"];
                            if (formFile is not null)
                            {
                                var fileResult = formFile.PrepareValidatedFile(Enums.MediaType.Video, _fileValidator);
                                if (!fileResult.IsValid)
                                {
                                    _logger.LogWarning("CreateCourse: lessonVideo_{Index} validation failed. Reason: {Reason}", i, fileResult.ErrorMessage);
                                    return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                                }

                                // get video duration
                                var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                                {
                                    await formFile.CopyToAsync(stream, cancellationToken);
                                }

                                try
                                {
                                    var mediaInfo = await FFmpeg.GetMediaInfo(tempFilePath);
                                    var durationInMinutes = (int)Math.Ceiling(mediaInfo.Duration.TotalMinutes);
                                    dto.Lessons[i].SetDuration(durationInMinutes);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "CreateLiveCourse: Failed to get video duration for lesson {Index}.", i);
                                    return BadRequest(new GeneralResult(false, messages.GetInvalidVideoDurationMessage(i), null, ErrorType.Validation));
                                }

                                dto.Lessons[i].FileUrl = fileResult.UniqueName;

                                System.IO.File.Delete(tempFilePath);
                                // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "lesson-videos");
                            }

                            // # attachments files -*-  -*-  -*-
                            if (dto.Lessons[i].Attachments is not null)
                            {
                                for (int j = 0; j < dto.Lessons[i].Attachments.Count; j++)
                                {
                                    var attachFile = Request.Form.Files[$"attachment_{i}_{j}"];
                                    if (attachFile is not null)
                                    {
                                        var attachResult = attachFile.PrepareValidatedFile(Enums.MediaType.Document, _fileValidator);
                                        if (!attachResult.IsValid)
                                        {
                                            _logger.LogWarning("CreateCourse: attachment_{I}_{J} validation failed. Reason: {Reason}", i, j, attachResult.ErrorMessage);
                                            return BadRequest(new GeneralResult(false, attachResult.ErrorMessage!, null, ErrorType.Validation));
                                        }

                                        dto.Lessons[i].Attachments[j].FileUrl = attachResult.UniqueName;

                                        // await _fileStorage.UploadAsync(...);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "CreateCourse: Failed to deserialize CourseJson.");
                    return BadRequest(new GeneralResult(false, messages.MsgInvalidJsonFormat, null, ErrorType.InvalidData));
                }

                var invalidRefCheck = await dto.ProgramId.CheckIfEntityExistsAsync<TrainingProgram>(_extendedBaseService, _logger);
                if (invalidRefCheck != null) return invalidRefCheck;

                var result = await courseService.CreateCourseWithContentAsync(dto, cancellationToken);
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
                _logger.LogError(ex, "An Unexpected error has occurred while creating a program course.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Creating a program course."), Data = null });
            }
        }

        /// <summary>
        /// Updates an existing program course with new data.
        /// </summary>
        /// <param name="courseId">Identifier of the course to be updated.</param>
        /// <param name="formDto">Data transfer object containing the updated course information.</param>
        /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// - 200 OK if the update succeeds.
        /// - 400 Bad Request if the input data is invalid.
        /// - 401 Unauthorized if the user is not authorized.
        /// - 404 Not Found if the course does not exist.
        /// - 500 Internal Server Error for unexpected errors.
        /// </returns>
        [HttpPatch("update/")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.ProgramCourseAdminPermissions.Update)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromQuery] int courseId, [FromForm] CourseUpdateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                string? newLogoFileName = null;
                if (formDto.Logo is not null)
                {
                    var fileResult = formDto.Logo.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                    if (!fileResult.IsValid)
                    {
                        _logger.LogWarning("UpdateCourse: Logo validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                    }

                    newLogoFileName = fileResult.UniqueName;

                    // TODO: upload file
                    // await _fileStorage.UploadAsync(fileResult.Stream, newLogoFileName, "courses");
                }

                var dto = new CourseUpdateDto
                {
                    ProgramId = formDto.ProgramId,
                    Name = formDto.Name,
                    Description = formDto.Description,
                    Order = formDto.Order,
                    Logo = newLogoFileName,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                var result = await courseService.UpdateCourseAsync(courseId, dto, cancellationToken);
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
                _logger.LogError(ex, "An Unexpected error has occurred while updating program course.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Updating program course."), Data = null });
            }
        }

        /// <summary>
        /// Deletes a specific program course and all its associated content by its ID.
        /// </summary>
        /// <param name="courseId">Identifier of the course to be deleted.</param>
        /// <param name="cancellationToken">Token to handle task cancellation.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// - 200 OK if the deletion is successful.
        /// - 400 Bad Request if the request is invalid.
        /// - 401 Unauthorized if the user lacks the proper credentials.
        /// - 404 Not Found if the course is not found.
        /// - 500 Internal Server Error if an unexpected error occurs.
        /// </returns>
        [HttpDelete("delete/")]
        [RequiredPermission(Permissions.ProgramCourseAdminPermissions.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete([FromQuery] int courseId, CancellationToken cancellationToken)
        {
            var transactionId = Guid.NewGuid();

            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (courseId <= 0)
                {
                    return BadRequest(new GeneralResult { IsSuccess = false, Message = messages.MsgIdInvalid });
                }

                _logger.LogInformation("Transaction {TransactionId}: Starting soft delete for entity ID {Id}.", transactionId, courseId);

                var result = await _deleteService.SoftDeleteCascadeAsync<ProgramCourse>(courseId);
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
                _logger.LogError(ex, "An Unexpected error has occurred while deleting one course.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" deleting one course."), Data = null });
            }
        }
    }
}
