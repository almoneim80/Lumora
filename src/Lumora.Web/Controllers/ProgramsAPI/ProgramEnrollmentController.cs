using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Web.Controllers.ProgramsAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.ProgramEnrollmentRoles)]
    public class ProgramEnrollmentController(
        EnrollmentMessage messages,
        IEnrollmentService enrollmentService,
        ILogger<ProgramEnrollmentController> logger) : AuthenticatedController
    {
        /// <summary>
        /// Enrolls the current user into the specified training program.
        /// </summary>
        /// <param name="programId">The ID of the program to enroll in.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// An IActionResult indicating the result:
        /// <list type="bullet">
        ///   <item><description>Status 200 OK if enrollment is successful.</description></item>
        ///   <item><description>Status 400 BadRequest if input is invalid.</description></item>
        ///   <item><description>Status 401 Unauthorized if the user is not authenticated.</description></item>
        ///   <item><description>Status 404 NotFound if the specified program does not exist.</description></item>
        ///   <item><description>Status 409 Conflict if the user is already enrolled.</description></item>
        ///   <item><description>Status 500 InternalServerError for unexpected server errors.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("enroll/")]
        [RequiredPermission(Permissions.ProgramEnrollmentPermissions.Enroll)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Enroll([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await enrollmentService.EnrollInProgramAsync(programId, CurrentUserId!, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.Conflict => Conflict(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while enrolling in program.");
                return StatusCode(500, new GeneralResult(false, messages.GetUnexpectedErrorMessage("Enroll in program."), null));
            }
        }

        /// <summary>
        /// Unenrolls a specific user from the specified training program.
        /// </summary>
        /// <param name="programId">The program's unique identifier.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// GeneralResult indicating success or failure of the unenrollment operation.</returns>
        [HttpPost("unenroll/")]
        [RequiredPermission(Permissions.ProgramEnrollmentPermissions.Unenroll)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnenrollFromProgram([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await enrollmentService.UnenrollFromProgramAsync(CurrentUserId!, programId, cancellationToken);
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
                logger.LogError(ex, "Error while unenrolling user from program.");
                return StatusCode(500, new GeneralResult(false, messages.GetUnexpectedErrorMessage("unenroll from program"), null));
            }
        }

        /// <summary>
        /// Checks if a current user is enrolled in the given program.
        /// </summary>
        /// <param name="programId">The program's unique identifier.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// GeneralResult containing boolean indicating enrollment status.</returns>
        [HttpGet("is-enrolled/")]
        [RequiredPermission(Permissions.ProgramEnrollmentPermissions.CheckEnrollment)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IsUserEnrolled([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await enrollmentService.IsUserEnrolledAsync(CurrentUserId!, programId, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking enrollment status.");
                return StatusCode(500, new GeneralResult<bool>(false, messages.GetUnexpectedErrorMessage("Check enrollment status"), false));
            }
        }

        /// <summary>
        /// Retrieves detailed enrollment information of a specific user for a given program.
        /// </summary>
        /// <param name="programId">The program's unique identifier.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// GeneralResult containing user's enrollment details.</returns>
        [HttpGet("user/enrolled/info/")]
        [RequiredPermission(Permissions.ProgramEnrollmentPermissions.GetEnrollmentInfo)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<EnrollmentWithUserData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<EnrollmentWithUserData>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<EnrollmentWithUserData>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserEnrollmentInfo([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await enrollmentService.GetUserEnrollmentInfoAsync(CurrentUserId!, programId, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving enrollment info.");
                return StatusCode(500, new GeneralResult<EnrollmentWithUserData>(false, messages.GetUnexpectedErrorMessage("get enrollment info"), null));
            }
        }
    }
}
