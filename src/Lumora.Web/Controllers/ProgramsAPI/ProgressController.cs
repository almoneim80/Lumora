using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;
namespace Lumora.Web.Controllers.ProgramsAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.ProgressRoles)]
    public class ProgressController(
        ProgressMessage messages,
        IProgressService progressService,
        ILogger<ProgressController> logger) : AuthenticatedController
    {
        /// <summary>
        /// Marks a specified course lesson as completed for the current user based on the provided progress data.
        /// </summary>
        /// <param name="lessonId">The ID of the lesson to mark as completed.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// <list type="bullet">
        ///   <item><description>Status 200 OK with success result when the lesson is marked as completed successfully.</description></item>
        ///   <item><description>Status 400 BadRequest if the DTO is invalid or request data is malformed.</description></item>
        ///   <item><description>Status 401 Unauthorized if the user is not authenticated or lacks proper permissions.</description></item>
        ///   <item><description>Status 404 NotFound if the specified lesson or related data is not found.</description></item>
        ///   <item><description>Status 500 InternalServerError if an unexpected error occurs during processing.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("lesson/mark-complete/")]
        [RequiredPermission(Permissions.ProgressPermissions.MarkLessonCompleted)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkLessonCompleted([FromQuery] int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.MarkLessonCompletedAsync(lessonId, CurrentUserId!, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while marking a course lesson as completed.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Marking a course lesson as completed."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves the progress of the current user for a specified lesson.
        /// </summary>
        /// <param name="lessonId">The unique identifier of the lesson to retrieve progress for.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing lesson progress information wrapped in <see cref="GeneralResult{TrainingProgramDetailsDto}"/>.</returns>
        [HttpGet("lesson/get-progress/")]
        [RequiredPermission(Permissions.ProgressPermissions.GetLessonProgress)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<TrainingProgramFullDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLessonProgress([FromQuery] int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.GetLessonProgressAsync(CurrentUserId!, lessonId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting lesson progress.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting lesson progress."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves a list of lessons completed by the current user in the specified course.
        /// </summary>
        /// <param name="courseId">The unique identifier of the course.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing completed lessons information wrapped in <see cref="GeneralResult{TrainingProgramDetailsDto}"/>.</returns>
        [HttpGet("lesson/get-completed/")]
        [RequiredPermission(Permissions.ProgressPermissions.GetCompletedLessons)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<TrainingProgramFullDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompletedLessons([FromQuery] int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.GetCompletedLessonsAsync(CurrentUserId!, courseId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting completed lessons.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting completed lessons."), Data = null });
            }
        }

        /// <summary>
        /// Updates the progress of a user in a specific training course.
        /// </summary>
        /// <param name="courseId">The unique identifier of the training course.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An IActionResult indicating the result of the update operation.</returns>
        [HttpPatch("course/update-progress/")]
        [RequiredPermission(Permissions.ProgressPermissions.UpdateCourseProgress)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCourseProgress([FromQuery] int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.UpdateProgramCourseProgressAsync(courseId, CurrentUserId!, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while updating course progress.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Updating course progress."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves the progress of the current user in the specified course.
        /// </summary>
        /// <param name="courseId">The unique identifier of the course.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An IActionResult with the user's course progress wrapped in a GeneralResult object.</returns>
        [HttpGet("course/get-progress/")]
        [RequiredPermission(Permissions.ProgressPermissions.GetCourseProgress)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<TrainingProgramFullDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseProgress([FromQuery] int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.GetProgramCourseProgressAsync(CurrentUserId!, courseId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting course progress.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting course progress."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves the progress statuses of the current user's courses.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// A list of course progress statuses wrapped in a GeneralResult.</returns>
        [HttpGet("course/user-progress")]
        [RequiredPermission(Permissions.ProgressPermissions.GetUserCoursesProgress)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<TrainingProgramFullDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserCoursesProgress(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.GetUserCoursesProgressAsync(CurrentUserId!, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting user course progress.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting user course progress."), Data = null });
            }
        }

        /// <summary>
        /// Updates the user's progress in a specific training program.
        /// </summary>
        /// <param name="programId">The unique identifier of the training program.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// IActionResult with the result of the update operation.</returns>
        [HttpPatch("program/update-progress/")]
        [RequiredPermission(Permissions.ProgressPermissions.UpdateProgramProgress)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProgramProgress([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.UpdateProgramProgressAsync(programId, CurrentUserId!, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while updating program progress.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Updating program progress."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves the user's progress in a specific training program.
        /// </summary>
        /// <param name="programId">The unique identifier of the training program.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// GeneralResult object containing progress data for the specified program.</returns>
        [HttpGet("program/get-progress/")]
        [RequiredPermission(Permissions.ProgressPermissions.GetProgramProgress)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<TrainingProgramFullDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProgramProgress([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.GetProgramProgressAsync(CurrentUserId!, programId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting program progress.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting program progress."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves the user's overall progress in all training programs.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// GeneralResult object containing the user's training programs progress.</returns>
        [HttpGet("program/user-progress")]
        [RequiredPermission(Permissions.ProgressPermissions.GetUserProgramsProgress)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<TrainingProgramFullDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProgramsProgress(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.GetUserProgramsProgressAsync(CurrentUserId!, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting user program progress.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting user program progress."), Data = null });
            }
        }

        /// <summary>
        /// Starts a new session for a specific lesson.
        /// </summary>
        /// <param name="lessonId">The ID of the lesson to start a session for.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// IActionResult indicating the status of the session start.</returns>
        [HttpPost("lesson/session/start")]
        [RequiredPermission(Permissions.ProgressPermissions.StartLessonSession)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartLessonSession([FromQuery] int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.StartLessonSessionAsync(CurrentUserId!, lessonId, cancellationToken);
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
                logger.LogError(ex, "An error occurred while starting lesson session.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("starting lesson session"), Data = null });
            }
        }

        /// <summary>
        /// Ends an active session for a specific lesson.
        /// </summary>
        /// <param name="lessonId">The ID of the lesson to end the session for.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// IActionResult indicating the outcome of the session termination.</returns>
        [HttpPost("lesson/session/end")]
        [RequiredPermission(Permissions.ProgressPermissions.EndLessonSession)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndLessonSession([FromQuery] int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.EndLessonSessionAsync(CurrentUserId!, lessonId, cancellationToken);
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
                logger.LogError(ex, "An error occurred while ending lesson session.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("ending lesson session"), Data = null });
            }
        }

        /// <summary>
        /// Synchronizes all user progress records for a specified training program.
        /// </summary>
        /// <param name="programId">The ID of the training program to synchronize progress for.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// IActionResult indicating the synchronization result.</returns>
        [HttpPost("program/sync-progress")]
        [RequiredPermission(Permissions.ProgressPermissions.SyncProgramProgress)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SyncProgramProgress([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await progressService.SyncAllUserProgressForProgramAsync(programId, cancellationToken);
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
                logger.LogError(ex, "An error occurred while syncing progress for program.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("syncing progress"), Data = null });
            }
        }
    }
}
