using Up.Extensions;
using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Web.Controllers.ProgramsAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.ProgramCourseRoles)]
    public class ProgramCourseController(
        CourseMessage messages,
        IExtendedBaseService extendedBaseService,
        IProgramCourseService courseService,
        ICascadeDeleteService deleteService,
        ILogger<ProgramCourseController> logger) : AuthenticatedController
    {
        private readonly ICascadeDeleteService _deleteService = deleteService;
        private readonly IExtendedBaseService _extendedBaseService = extendedBaseService;
        private readonly ILogger<ProgramCourseController> _logger = logger;

        /// <summary>
        /// Retrieves a specific program course along with its associated lessons and content by its ID.
        /// </summary>
        /// <param name="courseId">Identifier of the course to retrieve.</param>
        /// <param name="cancellationToken">Token for cancelling the asynchronous request.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult{TrainingProgramDetailsDto}"/>:
        /// - 200 OK with course details if found.
        /// - 400 Bad Request if the request is malformed.
        /// - 401 Unauthorized if the user is not authorized.
        /// - 404 Not Found if the course does not exist.
        /// - 500 Internal Server Error for any unhandled exception.
        /// </returns>
        [HttpGet("get-one/")]
        [RequiredPermission(Permissions.ProgramCoursePermissions.ViewCourseDetails)]
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

                var result = await courseService.GetCourseWithContentByIdAsync(courseId, cancellationToken);
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
                _logger.LogError(ex, "An Unexpected error has occurred while getting one course.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting one course."), Data = null });
            }
        }
    }
}
