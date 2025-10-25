using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Web.Controllers.ProgramsAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.LessonRoles)]
    public class LessonAttachmentController(
        ILessonAttachmentService lessonAttachmentService,
        LessonAttachmentMessage messages,
        ILogger<LessonAttachmentController> logger) : AuthenticatedController
    {
        /// <summary>
        /// Retrieves a single lesson attachment by its ID.
        /// </summary>
        /// <param name="attachmentId">The unique identifier of the lesson attachment.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a <see cref="GeneralResult{LessonAttachmentDetailsDto}"/>:
        /// <list type="bullet">
        ///   <item><description>Status 200 OK with the attachment details if found.</description></item>
        ///   <item><description>Status 400 BadRequest if input is invalid.</description></item>
        ///   <item><description>Status 401 Unauthorized if the user is not authenticated.</description></item>
        ///   <item><description>Status 404 NotFound if the attachment is not found.</description></item>
        ///   <item><description>Status 500 InternalServerError if an unexpected error occurs.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("get-by-id/")]
        [RequiredPermission(Permissions.LessonAttachmentPermissions.ViewAttachmentById)]
        [ProducesResponseType(typeof(GeneralResult<LessonAttachmentDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery] int attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await lessonAttachmentService.GetAttachmentByIdAsync(attachmentId, cancellationToken);
                if (!result.IsSuccess)
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
                logger.LogError(ex, "An unexpected error occurred while retrieving the lesson attachment by ID.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("retrieving lesson attachment"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves all attachments for a specific lesson by lesson ID.
        /// </summary>
        /// <param name="lessonId">The unique identifier of the lesson.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description>Status 200 OK with a list of attachment details if found.</description></item>
        ///   <item><description>Status 400 BadRequest if input is invalid.</description></item>
        ///   <item><description>Status 401 Unauthorized if the user is not authenticated.</description></item>
        ///   <item><description>Status 404 NotFound if the lesson or its attachments are not found.</description></item>
        ///   <item><description>Status 500 InternalServerError if an unexpected error occurs.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("get-by-lesson-id")]
        [RequiredPermission(Permissions.LessonAttachmentPermissions.ViewAttachmentsByLesson)]
        [ProducesResponseType(typeof(GeneralResult<List<LessonAttachmentDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByLessonId([FromQuery] int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await lessonAttachmentService.GetAttachmentsByLessonIdAsync(lessonId, cancellationToken);
                if (!result.IsSuccess)
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
                logger.LogError(ex, "An unexpected error occurred while retrieving attachments by lesson ID.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("retrieving lesson attachments"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Increments the open count of a specific lesson attachment.
        /// </summary>
        /// <param name="attachmentId">The ID of the attachment to increment the open count for.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// <list type="bullet">
        ///   <item><description>Status 200 OK if the operation is successful.</description></item>
        ///   <item><description>Status 400 BadRequest if the input is invalid.</description></item>
        ///   <item><description>Status 401 Unauthorized if the user is not authenticated.</description></item>
        ///   <item><description>Status 404 NotFound if the attachment is not found.</description></item>
        ///   <item><description>Status 500 InternalServerError if an unexpected error occurs.</description></item>
        /// </list>
        /// </returns>
        [HttpPatch("increment-open-count")]
        [RequiredPermission(Permissions.LessonAttachmentPermissions.IncrementOpenCount)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IncrementOpenCount([FromQuery] int attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await lessonAttachmentService.IncrementOpenCountAsync(attachmentId, cancellationToken);
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
                logger.LogError(ex, "An unexpected error occurred while incrementing open count for attachment ID {AttachmentId}.", attachmentId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("incrementing open count"),
                    Data = null
                });
            }
        }
    }
}
