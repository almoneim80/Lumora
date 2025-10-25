using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Web.Controllers.ProgramsAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.TrainingProgramRoles)]
    public class ProgramsManegmentController(
        ITrainingProgramService programService,
        CourseMessage messages,
        ICascadeDeleteService deleteService,
        ILogger<ProgramsManegmentController> logger) : AuthenticatedController
    {
        private readonly ICascadeDeleteService _deleteService = deleteService;

        // ===== Program Retrieval =====

        /// <summary>
        /// Retrieves a list of all available training programs.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the retrieval process.</param>
        /// <returns>
        /// Returns:
        /// - <c>200 OK</c> with the list of programs.
        /// - <c>400 Bad Request</c> for invalid queries.
        /// - <c>404 Not Found</c> if no programs are available.
        /// - <c>500 Internal Server Error</c> for unexpected errors.
        /// </returns>
        [HttpGet("all")]
        [RequiredPermission(Permissions.ProgramsManegmentPermissions.GetAll)]
        [ProducesResponseType(typeof(GeneralResult<List<TrainingProgramFullDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await programService.GetAllProgramsAsync(cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting all programs.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting all programs."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific training program by its identifier.
        /// </summary>
        /// <param name="programId">The unique identifier of the training program to retrieve.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// Returns:
        /// - <c>200 OK</c> with the program details.
        /// - <c>400 Bad Request</c> for invalid parameters.
        /// - <c>404 Not Found</c> if the program does not exist.
        /// - <c>500 Internal Server Error</c> for processing errors.
        /// </returns>
        [HttpGet("get-one/")]
        [RequiredPermission(Permissions.ProgramsManegmentPermissions.GetOne)]
        [ProducesResponseType(typeof(GeneralResult<TrainingProgramFullDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOne([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await programService.GetOneProgramAsync(programId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting one program.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting one program."), Data = null });
            }
        }

        // ===== Program Utilities =====

        /// <summary>
        /// Calculates and retrieves the completion rate of the specified training program for the current user.
        /// </summary>
        /// <param name="programId">The identifier of the training program to evaluate.</param>
        /// <param name="cancellationToken">Token for canceling the operation if needed.</param>
        /// <returns>
        /// Returns:
        /// - <c>200 OK</c> with the user's completion rate.
        /// - <c>400 Bad Request</c> if the input is invalid.
        /// - <c>404 Not Found</c> if the program does not exist or the user has no data.
        /// - <c>500 Internal Server Error</c> on system failure.
        /// </returns>
        [HttpGet("complete-rate/")]
        [RequiredPermission(Permissions.ProgramsManegmentPermissions.CompletionRate)]
        [ProducesResponseType(typeof(GeneralResult<ProgramCompletionData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompletionRate([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await programService.ProgramCompletionRateAsync(programId, CurrentUserId!, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting the completion rate of one program.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting the completion rate of one program."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves all courses associated with a specific training program.
        /// </summary>
        /// <param name="programId">The unique identifier of the program whose courses are to be listed.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>
        /// Returns:
        /// - <c>200 OK</c> with the list of related courses.
        /// - <c>400 Bad Request</c> for invalid requests.
        /// - <c>404 Not Found</c> if the program or its courses do not exist.
        /// - <c>500 Internal Server Error</c> for unexpected exceptions.
        /// </returns>
        [HttpGet("courses")]
        [RequiredPermission(Permissions.ProgramsManegmentPermissions.GetCourses)]
        [ProducesResponseType(typeof(GeneralResult<List<CourseFullDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourses([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await programService.GetProgramCoursesAsync(programId, cancellationToken);
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
                logger.LogError(ex, "An Unexpected error has occurred while getting the program courses.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" Getting the program courses."), Data = null });
            }
        }
    }
}
