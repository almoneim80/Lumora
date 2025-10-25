using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Web.Controllers.ProgramsAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.LessonRoles)]
    public class LessonController(
        CourseLessonMessages messages,
        ICourseLessonService lessonService,
        ICascadeDeleteService deleteService,
        ILogger<LessonController> logger) : AuthenticatedController
    {
        private readonly ICascadeDeleteService _deleteService = deleteService;

        /// <summary>
        /// Retrieves all lessons and their corresponding content associated with a specific course.
        /// </summary>
        /// <param name="courseId">The identifier of the course whose lessons are to be fetched.</param>
        /// <param name="cancellationToken">Token for handling cancellation of the asynchronous operation.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult{TrainingProgramDetailsDto}"/>:
        /// - 200 OK along with lesson data if found.
        /// - 400 Bad Request if the request is malformed.
        /// - 401 Unauthorized if the user is not authenticated.
        /// - 404 Not Found if the specified course or its lessons cannot be located.
        /// - 500 Internal Server Error for unexpected runtime issues.
        /// </returns>
        [HttpGet("get-one/")]
        [RequiredPermission(Permissions.LessonPermissions.ViewLessonsByCourse)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<TrainingProgramFullDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOne([FromQuery] int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await lessonService.GetLessonsWithContentByCourseIdAsync(courseId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting one course.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting one course."), Data = null });
            }
        }
    }
}
