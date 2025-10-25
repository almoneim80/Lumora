using Microsoft.AspNetCore.Identity;
using System.Text.Json;
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
    public class LessonAdminController(
            CourseLessonMessages messages,
            ICourseLessonService lessonService,
            ICascadeDeleteService deleteService,
            FileValidatorHelper fileValidator,
            ILogger<LessonController> logger) : AuthenticatedController
    {
        private readonly ICascadeDeleteService _deleteService = deleteService;
        private readonly FileValidatorHelper _fileValidator = fileValidator;

        /// <summary>
        /// Creates a new lesson and associates it with related content within a course.
        /// </summary>
        /// /// <param name="courseId">The identifier of the course to which the lesson is associated.</param>
        /// <param name="formDto">An object that encapsulates the lesson's metadata and its associated content to be created.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation if required.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// - 200 OK if the lesson is created successfully.
        /// - 400 Bad Request if the provided data is invalid.
        /// - 401 Unauthorized if the request is made by an unauthorized user.
        /// - 404 Not Found if a referenced course or related data is missing.
        /// - 500 Internal Server Error for unhandled execution errors.
        /// </returns>
        [HttpPost("add-single/")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.LessonAdminPermissions.Create)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromQuery] int courseId, [FromForm] LessonWithContentCreateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                LessonsWithContentCreateDto? dto;
                try
                {
                    dto = JsonSerializer.Deserialize<LessonsWithContentCreateDto>(formDto.LessonJson!, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (dto == null)
                    {
                        logger.LogWarning("CreateLesson: Deserialization failed, dto is null.");
                        return BadRequest(new GeneralResult(false, messages.MsgDtoNull, null, ErrorType.InvalidData));
                    }

                    // #lesson video -*-  -*-  -*- 
                    if (formDto.LessonVideo is not null)
                    {
                        var videoResult = formDto.LessonVideo.PrepareValidatedFile(Enums.MediaType.Video, _fileValidator);
                        if (!videoResult.IsValid)
                        {
                            logger.LogWarning("CreateLesson: LessonVideo validation failed. Reason: {Reason}", videoResult.ErrorMessage);
                            return BadRequest(new GeneralResult(false, videoResult.ErrorMessage!, null, ErrorType.Validation));
                        }

                        // # get duration from video -*-  -*-  -*-
                        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        using (var stream = new FileStream(tempPath, FileMode.Create))
                        {
                            await formDto.LessonVideo.CopyToAsync(stream, cancellationToken);
                        }

                        var info = await FFmpeg.GetMediaInfo(tempPath);
                        dto.SetDuration((int)Math.Ceiling(info.Duration.TotalMinutes));
                        dto.FileUrl = videoResult.UniqueName;

                        System.IO.File.Delete(tempPath);

                        // await _fileStorage.UploadAsync(videoResult.Stream, videoResult.UniqueName, "lesson-videos");
                    }

                    // #lesson attachments -*-  -*-  -*-
                    if (formDto.Attachments != null && dto.Attachments != null)
                    {
                        if (formDto.Attachments.Count != dto.Attachments.Count)
                        {
                            logger.LogWarning("CreateLesson: Mismatch between attachment files and JSON list.");
                            return BadRequest(new GeneralResult(false, messages.MsgAttachmentCountMismatch, null, ErrorType.InvalidData));
                        }

                        for (int i = 0; i < formDto.Attachments.Count; i++)
                        {
                            var file = formDto.Attachments[i];
                            var fileResult = file.PrepareValidatedFile(Enums.MediaType.Document, _fileValidator);
                            if (!fileResult.IsValid)
                            {
                                logger.LogWarning("CreateLesson: Attachment_{Index} validation failed. Reason: {Reason}", i, fileResult.ErrorMessage);
                                return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                            }

                            dto.Attachments[i].FileUrl = fileResult.UniqueName;

                            // await _fileStorage.UploadAsync(result.Stream, result.UniqueName, "lesson-attachments");
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "CreateLesson: Failed to deserialize LessonJson.");
                    return BadRequest(new GeneralResult(false, messages.MsgInvalidJsonFormat, null, ErrorType.InvalidData));
                }

                var result = await lessonService.CreateLessonWithContentAsync(courseId, dto, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while creating a course lesson.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Creating a course lesson."), Data = null });
            }
        }

        /// <summary>
        /// Updates an existing lesson and its content using the specified identifier.
        /// </summary>
        /// <param name="lessonId">The unique identifier of the lesson to update.</param>
        /// <param name="formDto">An object containing the updated data for the lesson and its content.</param>
        /// <param name="cancellationToken">Token to propagate cancellation of the asynchronous task.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// - 200 OK if the update is completed successfully.
        /// - 400 Bad Request if the input data fails validation.
        /// - 401 Unauthorized if the caller lacks proper authorization.
        /// - 404 Not Found if the lesson with the specified ID does not exist.
        /// - 500 Internal Server Error in case of an unexpected failure during processing.
        /// </returns>
        [HttpPatch("update/")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.LessonAdminPermissions.Update)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromQuery] int lessonId, [FromForm] LessonUpdateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                LessonUpdateDto dto;

                try
                {
                    dto = new LessonUpdateDto
                    {
                        CourseId = formDto.CourseId,
                        Name = formDto.Name,
                        Order = formDto.Order,
                        Description = formDto.Description
                    };

                    if (formDto.LessonVideo is not null)
                    {
                        var fileResult = formDto.LessonVideo.PrepareValidatedFile(Enums.MediaType.Video, _fileValidator);
                        if (!fileResult.IsValid)
                        {
                            logger.LogWarning("UpdateLesson: Lesson video validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                            return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                        }

                        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        using (var stream = new FileStream(tempFilePath, FileMode.Create))
                        {
                            await formDto.LessonVideo.CopyToAsync(stream, cancellationToken);
                        }

                        var mediaInfo = await FFmpeg.GetMediaInfo(tempFilePath);
                        var durationInMinutes = (int)Math.Ceiling(mediaInfo.Duration.TotalMinutes);
                        dto.SetDuration(durationInMinutes);
                        dto.FileUrl = fileResult.UniqueName;

                        System.IO.File.Delete(tempFilePath);

                        // TODO: Upload the video to storage
                        // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "lesson-videos");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "UpdateLesson: Error occurred while processing video file.");
                    return StatusCode(500, new GeneralResult(false, messages.GetUnexpectedErrorMessage("processing lesson video"), null, ErrorType.InternalServerError));
                }

                var result = await lessonService.UpdateLessonAsync(lessonId, dto, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while updating course lesson.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Updating course lesson."), Data = null });
            }
        }

        /// <summary>
        /// Deletes a specific lesson and all its associated content from the system.
        /// </summary>
        /// <param name="lessonId">The identifier of the lesson to be deleted.</param>
        /// <param name="cancellationToken">Token to support cancellation of the delete operation.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// - 200 OK if the lesson is deleted successfully.
        /// - 400 Bad Request if the request parameters are invalid.
        /// - 401 Unauthorized if the user does not have permission.
        /// - 404 Not Found if the specified lesson does not exist.
        /// - 500 Internal Server Error for unhandled exceptions during execution.
        /// </returns>
        [HttpDelete("delete/")]
        [RequiredPermission(Permissions.LessonAdminPermissions.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete([FromQuery] int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (lessonId <= 0)
                {
                    return BadRequest(new GeneralResult { IsSuccess = false, Message = messages.MsgIdInvalid });
                }

                var result = await lessonService.SoftDeleteLessonAsync(lessonId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while deleting one course lesson.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" deleting one course lesson."), Data = null });
            }
        }
    }
}
