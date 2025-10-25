using System.Text.Json;
using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;
using Xabe.FFmpeg;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace Lumora.Web.Controllers.ProgramsAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class ProgramManegmentAdminController(
        ITrainingProgramService programService,
        CourseMessage messages,
        ICascadeDeleteService deleteService,
        FileValidatorHelper fileValidator,
        ILogger<ProgramsManegmentController> logger) : AuthenticatedController
    {
        private readonly ICascadeDeleteService _deleteService = deleteService;
        private readonly FileValidatorHelper _fileValidator = fileValidator;
        // ===== Program Management =====

        /// <summary>
        /// Creates a new training program using the provided data transfer object.
        /// Validates the user authorization and model state before invoking the creation logic.
        /// </summary>
        /// <param name="formDto">An object containing the data required to create a new training program, including titles, dates, and configuration settings.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted or times out.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing:
        /// - <c>200 OK</c> if the training program is successfully created.
        /// - <c>400 Bad Request</c> if the input data is invalid or fails model validation.
        /// - <c>404 Not Found</c> if any dependent resource referenced in the input is missing.
        /// - <c>500 Internal Server Error</c> if an unexpected error occurs during program creation.
        /// </returns>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.ProgramAdminPermissions.Create)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] TrainingProgramCreateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                TrainingProgramCreateDto? dto;
                try
                {
                    dto = JsonSerializer.Deserialize<TrainingProgramCreateDto>(formDto.ProgramJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (dto == null)
                    {
                        logger.LogWarning("CreateProgram: Deserialization failed, dto is null.");
                        return BadRequest(new GeneralResult(false, messages.MsgDtoNull, null, ErrorType.InvalidData));
                    }

                    // # get program logo -*-  -*-  -*-
                    if (formDto.ProgramLogo is not null)
                    {
                        var fileResult = formDto.ProgramLogo.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                        if (!fileResult.IsValid)
                        {
                            logger.LogWarning("CreateProgram: Program logo validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                            return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                        }

                        dto.Logo = fileResult.UniqueName;

                        // مثال للرفع إلى التخزين:
                        // await _fileStorage.UploadAsync(fileResult.Stream, dto.Logo, "training-programs");
                    }

                    // # get trainer images -*-  -*-  -*-
                    for (int i = 0; i < dto.Trainers.Count; i++)
                    {
                        var formFile = Request.Form.Files[$"trainerImage_{i}"];
                        if (formFile is not null)
                        {
                            var fileResult = formFile.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                            if (!fileResult.IsValid)
                            {
                                logger.LogWarning("CreateProgram: TrainerImage_{Index} validation failed. Reason: {Reason}", i, fileResult.ErrorMessage);
                                return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                            }

                            dto.Trainers[i].ImageUrl = fileResult.UniqueName;

                            // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "trainer-images");
                        }
                    }

                    // # get course logos -*-  -*-  -*-
                    for (int i = 0; i < dto.ProgramCourses.Count; i++)
                    {
                        var formFile = Request.Form.Files[$"courseLogo_{i}"];
                        if (formFile is not null)
                        {
                            var fileResult = formFile.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                            if (!fileResult.IsValid)
                            {
                                logger.LogWarning("CreateProgram: CourseLogo_{Index} validation failed. Reason: {Reason}", i, fileResult.ErrorMessage);
                                return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                            }

                            dto.ProgramCourses[i].Logo = fileResult.UniqueName;

                            // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "course-logos");
                        }
                    }

                    // # get lesson videos -*-  -*-  -*-
                    for (int courseIndex = 0; courseIndex < dto.ProgramCourses.Count; courseIndex++)
                    {
                        var course = dto.ProgramCourses[courseIndex];

                        for (int lessonIndex = 0; lessonIndex < course.Lessons.Count; lessonIndex++)
                        {
                            var formFile = Request.Form.Files[$"lessonFile_{courseIndex}_{lessonIndex}"];
                            if (formFile is not null)
                            {
                                var fileResult = formFile.PrepareValidatedFile(Enums.MediaType.Video, _fileValidator);
                                if (!fileResult.IsValid)
                                {
                                    logger.LogWarning("CreateProgram: LessonFile_{Course}_{Lesson} validation failed. Reason: {Reason}", courseIndex, lessonIndex, fileResult.ErrorMessage);
                                    return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                                }

                                var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                                {
                                    await formFile.CopyToAsync(stream, cancellationToken);
                                }

                                var mediaInfo = await FFmpeg.GetMediaInfo(tempFilePath);
                                var durationInMinutes = (int)Math.Ceiling(mediaInfo.Duration.TotalMinutes);

                                course.Lessons[lessonIndex].SetDuration(durationInMinutes);
                                course.Lessons[lessonIndex].FileUrl = fileResult.UniqueName;

                                // Delete the temporary file.
                                System.IO.File.Delete(tempFilePath);

                                // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "lesson-files");
                            }
                        }
                    }

                    // # get lesson attachments -*-  -*-  -*-
                    for (int courseIndex = 0; courseIndex < dto.ProgramCourses.Count; courseIndex++)
                    {
                        var course = dto.ProgramCourses[courseIndex];

                        for (int lessonIndex = 0; lessonIndex < course.Lessons.Count; lessonIndex++)
                        {
                            var lesson = course.Lessons[lessonIndex];

                            for (int attachmentIndex = 0; attachmentIndex < lesson.LessonAttachments.Count; attachmentIndex++)
                            {
                                var formFile = Request.Form.Files[$"attachment_{courseIndex}_{lessonIndex}_{attachmentIndex}"];
                                if (formFile is not null)
                                {
                                    var fileResult = formFile.PrepareValidatedFile(Enums.MediaType.Document, _fileValidator);
                                    if (!fileResult.IsValid)
                                    {
                                        logger.LogWarning("CreateProgram: Attachment_{C}_{L}_{A} validation failed. Reason: {Reason}", courseIndex, lessonIndex, attachmentIndex, fileResult.ErrorMessage);
                                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                                    }

                                    lesson.LessonAttachments[attachmentIndex].FileUrl = fileResult.UniqueName;

                                    // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "lesson-attachments");
                                }
                            }
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "CreateProgram: Failed to deserialize programJson.");
                    return BadRequest(new GeneralResult(false, messages.MsgInvalidJsonFormat, null, ErrorType.InvalidData));
                }

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await programService.CreateProgramAsync(dto, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while creating a program.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Creating a program."), Data = null });
            }
        }

        /// <summary>
        /// Updates the data of an existing training program using the provided update DTO.
        /// Ensures user authorization and applies partial updates based on input fields.
        /// </summary>
        /// <param name="programId">The unique identifier of the training program to be updated.</param>
        /// <param name="formDto">An object containing the updated fields for the training program.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if needed.</param>
        /// <returns>
        /// Returns:
        /// - <c>200 OK</c> if the update is successful.
        /// - <c>400 Bad Request</c> if the input data is invalid.
        /// - <c>404 Not Found</c> if the training program does not exist.
        /// - <c>500 Internal Server Error</c> for unexpected failures.
        /// </returns>
        [HttpPatch("update")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.ProgramAdminPermissions.Update)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromQuery] int programId, [FromForm] TrainingProgramUpdateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                TrainingProgramUpdateDto? dto;

                try
                {
                    dto = JsonSerializer.Deserialize<TrainingProgramUpdateDto>(formDto.ProgramJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (dto == null)
                    {
                        logger.LogWarning("UpdateProgram: Deserialization failed, dto is null.");
                        return BadRequest(new GeneralResult(false, messages.MsgDtoNull, null, ErrorType.InvalidData));
                    }

                    if (formDto.ProgramLogo is not null)
                    {
                        var fileResult = formDto.ProgramLogo.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                        if (!fileResult.IsValid)
                        {
                            logger.LogWarning("UpdateProgram: Program logo validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                            return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                        }

                        dto.Logo = fileResult.UniqueName;

                        // TODO: remove this
                        // await _fileStorage.UploadAsync(fileResult.Stream, dto.Logo, "training-programs");
                    }

                    if (dto.Trainers is not null)
                    {
                        for (int i = 0; i < dto.Trainers.Count; i++)
                        {
                            var formFile = Request.Form.Files[$"trainerImage_{i}"];
                            if (formFile is not null)
                            {
                                var fileResult = formFile.PrepareValidatedFile(Enums.MediaType.Image, _fileValidator);
                                if (!fileResult.IsValid)
                                {
                                    logger.LogWarning("UpdateProgram: TrainerImage_{Index} validation failed. Reason: {Reason}", i, fileResult.ErrorMessage);
                                    return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                                }

                                dto.Trainers[i].ImageUrl = fileResult.UniqueName;

                                // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "trainer-images");
                            }
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "UpdateProgram: Failed to deserialize programJson.");
                    return BadRequest(new GeneralResult(false, messages.MsgInvalidJsonFormat, null, ErrorType.InvalidData));
                }

                var result = await programService.UpdateProgramAsync(programId, dto, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while updating one program.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Updating one program."), Data = null });
            }
        }

        /// <summary>
        /// Deletes a specific training program identified by its ID.
        /// Ensures authorization and performs a logical or physical removal depending on system design.
        /// </summary>
        /// <param name="programId">The identifier of the training program to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// Returns:
        /// - <c>200 OK</c> if the program was successfully deleted.
        /// - <c>400 Bad Request</c> if the request is malformed.
        /// - <c>404 Not Found</c> if the program does not exist.
        /// - <c>500 Internal Server Error</c> in case of system-level errors.
        /// </returns>
        [HttpDelete("delete/")]
        [RequiredPermission(Permissions.ProgramAdminPermissions.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete([FromQuery] int programId, CancellationToken cancellationToken)
        {
            var transactionId = Guid.NewGuid();

            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (programId <= 0)
                {
                    return BadRequest(new GeneralResult { IsSuccess = false, Message = messages.MsgIdInvalid });
                }

                logger.LogInformation("Transaction {TransactionId}: Starting soft delete for entity ID {Id}.", transactionId, programId);

                var result = await _deleteService.SoftDeleteCascadeAsync<TrainingProgram>(programId);
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
                logger.LogError(ex, "Transaction {TransactionId}: Unexpected error occurred while deleting entity ID {Id}.", transactionId, programId);
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" deleting a program."), Data = null });
            }
        }
    }
}
