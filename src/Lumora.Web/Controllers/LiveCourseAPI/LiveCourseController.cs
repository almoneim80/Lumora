using Lumora.DataAnnotations;
using Lumora.DTOs.LiveCourse;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.LiveCourseIntf;

namespace Lumora.Web.Controllers.LiveCourseAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.LiveCourseRoles)]
    public class LiveCourseController(
    ILogger<LiveCourseController> logger,
    LiveCourseMessage messages,
    ILiveCourseService liveCourseService) : AuthenticatedController
    {
        private readonly ILogger<LiveCourseController> _logger = logger;
        private readonly LiveCourseMessage _messages = messages;
        private readonly ILiveCourseService _liveCourseService = liveCourseService;

        /// <summary>
        /// Retrieves paginated list of live courses the user is enrolled in.
        /// </summary>
        [HttpGet("my-courses")]
        [RequiredPermission(Permissions.LiveCoursePermissions.GetMyCourses)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<LiveCourseListItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserCourses([FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _liveCourseService.GetUserCoursesAsync(CurrentUserId!, pagination, cancellationToken);
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
                _logger.LogError(ex, "LiveCourseController - GetUserCourses: Unexpected error while fetching enrolled courses for user {UserId}.", CurrentUserId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("fetching enrolled courses"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves a live course by its ID.
        /// </summary>
        [HttpGet("get-one")]
        [RequiredPermission(Permissions.LiveCoursePermissions.GetById)]
        [ProducesResponseType(typeof(GeneralResult<LiveCourseDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery] int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _liveCourseService.GetByIdAsync(courseId, cancellationToken);
                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseController - GetById: Unexpected error while retrieving course ID {CourseId}.", courseId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Get course"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves paginated list of live courses.
        /// </summary>
        [HttpGet("get-all")]
        [RequiredPermission(Permissions.LiveCoursePermissions.GetAll)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<LiveCourseListItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] LiveCourseFilterDto filter, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _liveCourseService.GetListAsync(filter, cancellationToken);

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
                _logger.LogError(ex, "LiveCourseController - GetList: Unexpected error occurred while retrieving live course list.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Get course list"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Enrolls a user into a specific live course using a paid payment item.
        /// </summary>
        [HttpPost("subscribe")]
        [RequiredPermission(Permissions.LiveCoursePermissions.SubscribeUser)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubscribeUser([FromQuery] int courseId, [FromQuery] int? paymentItemId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _liveCourseService.SubscribeUserAsync(courseId, CurrentUserId!, paymentItemId, cancellationToken);

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
                _logger.LogError(ex, "LiveCourseController - SubscribeUser: Unexpected error during subscription for course ID {CourseId}.", courseId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("subscribing to course"),
                    Data = null
                });
            }
        }
    }
}
