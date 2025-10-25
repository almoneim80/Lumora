using Microsoft.AspNetCore.Identity;
using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Web.Controllers.ProgramsAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class LessonAttachmentAdminController(
            ILessonAttachmentService lessonAttachmentService,
            LessonAttachmentMessage messages,
            FileValidatorHelper fileValidator,
            ILogger<LessonAttachmentController> logger) : AuthenticatedController
    {
        private readonly FileValidatorHelper _fileValidator = fileValidator;

        /// <summary>
        /// Creates a new lesson attachment for a course using the provided data transfer object.
        /// </summary>
        /// <param name="formDto">The DTO containing details for the lesson attachment to create, such as file information and associated lesson identifiers.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// <list type="bullet">
        ///   <item><description>Status 200 OK with success result when the attachment is created.</description></item>
        ///   <item><description>Status 400 BadRequest if the DTO is invalid or the request fails validation.</description></item>
        ///   <item><description>Status 401 Unauthorized if the user is not authenticated or authorized.</description></item>
        ///   <item><description>Status 404 NotFound if related resources (e.g., lesson) are not found.</description></item>
        ///   <item><description>Status 500 InternalServerError if an unexpected error occurs.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.LessonAttachmentAdminPermissions.Create)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] SingleLessonAttachmentFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (formDto.AttachmentFile == null || formDto.AttachmentFile.Length == 0)
                {
                    logger.LogWarning("CreateAttachment: No file was uploaded.");
                    return BadRequest(new GeneralResult(false, messages.MsgFileRequired, null, ErrorType.Validation));
                }

                var fileResult = formDto.AttachmentFile.PrepareValidatedFile(Enums.MediaType.Document, _fileValidator);
                if (!fileResult.IsValid)
                {
                    logger.LogWarning("CreateAttachment: File validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                    return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                }

                var dto = new SingleLessonAttachmentCreateDto
                {
                    LessonId = formDto.LessonId,
                    FileUrl = fileResult.UniqueName
                };

                //TODO: upload file
                // await _fileStorage.UploadAsync(fileResult.Stream, fileResult.UniqueName, "lesson-attachments");

                var result = await lessonAttachmentService.AddSingleAttachmentAsync(dto, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while creating a course lesson attachment.");
                return StatusCode(500,
                    new GeneralResult(false, messages.GetUnexpectedErrorMessage("Creating a course lesson attachment."), null));
            }
        }

        /// <summary>
        /// Updates an existing lesson attachment identified by the specified attachment ID using the provided update DTO.
        /// </summary>
        /// <param name="attachmentId">The unique identifier of the lesson attachment to update.</param>
        /// <param name="formDto">The DTO containing updated information for the lesson attachment.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// <list type="bullet">
        ///   <item><description>Status 200 OK with success result when the attachment is updated.</description></item>
        ///   <item><description>Status 400 BadRequest if the request fails validation or contains invalid data.</description></item>
        ///   <item><description>Status 401 Unauthorized if the user is not authenticated or authorized.</description></item>
        ///   <item><description>Status 404 NotFound if the attachment ID does not correspond to an existing resource.</description></item>
        ///   <item><description>Status 500 InternalServerError if an unexpected error occurs.</description></item>
        /// </list>
        /// </returns>
        [HttpPatch("update/")]
        [Consumes("multipart/form-data")]   
        [RequiredPermission(Permissions.LessonAttachmentAdminPermissions.Update)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromQuery] int attachmentId, [FromForm] LessonAttachmentUpdateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                string? uniqueFileName = null;

                if (formDto.File is not null)
                {
                    var fileResult = formDto.File.PrepareValidatedFile(Enums.MediaType.Document, _fileValidator);
                    if (!fileResult.IsValid)
                    {
                        logger.LogWarning("UpdateAttachment: File validation failed. Reason: {Reason}", fileResult.ErrorMessage);
                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));
                    }

                    uniqueFileName = fileResult.UniqueName;

                    // TODO: upload file
                    // await _fileStorage.UploadAsync(fileResult.Stream, uniqueFileName, "lesson-attachments");
                }

                var dto = new LessonAttachmentUpdateDto
                {
                    LessonId = formDto.LessonId,
                    FileUrl = uniqueFileName
                };

                var result = await lessonAttachmentService.UpdateAttachmentAsync(attachmentId, dto, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while updating course lesson attachment.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Updating a course lesson attachment."), Data = null });
            }
        }

        /// <summary>
        /// Deletes a lesson attachment identified by the specified attachment ID.
        /// </summary>
        /// <param name="attachmentId">The unique identifier of the lesson attachment to delete.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// <list type="bullet">
        ///   <item><description>Status 200 OK with success result when the attachment is deleted.</description></item>
        ///   <item><description>Status 400 BadRequest if the request fails or the attachment cannot be deleted.</description></item>
        ///   <item><description>Status 401 Unauthorized if the user is not authenticated or authorized.</description></item>
        ///   <item><description>Status 404 NotFound if the attachment ID does not correspond to an existing resource.</description></item>
        ///   <item><description>Status 500 InternalServerError if an unexpected error occurs.</description></item>
        /// </list>
        /// </returns>
        [HttpDelete("delete/")]
        [RequiredPermission(Permissions.LessonAttachmentAdminPermissions.Delete)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] int attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await lessonAttachmentService.DeleteSingleAttachmentAsync(attachmentId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while deleting one lesson attachment.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" deleting one lesson attachment."), Data = null });
            }
        }
    }
}
