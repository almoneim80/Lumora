using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Web.Controllers.ProgramsAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class ProgramEnrollmentAdminController(
        EnrollmentMessage messages,
        IEnrollmentService enrollmentService,
        ILogger<ProgramEnrollmentAdminController> logger) : AuthenticatedController
    {
        /// <summary>
        /// Retrieves all users enrolled in the specified training program.
        /// </summary>
        /// <param name="programId">ID of the training program.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// IActionResult with list of enrolled users wrapped in GeneralResult.</returns>
        [HttpGet("enrolled-users/")]
        [RequiredPermission(Permissions.ProgramEnrollmentAdminPermissions.GetEnrolledUsers)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<List<EnrollmentWithUserData>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<List<EnrollmentWithUserData>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<List<EnrollmentWithUserData>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEnrolledUsers([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await enrollmentService.GetEnrolledUsersAsync(programId, cancellationToken);
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
                logger.LogError(ex, "Error retrieving enrolled users.");
                return StatusCode(500, new GeneralResult(false, messages.GetUnexpectedErrorMessage("Retrieve enrolled users."), null));
            }
        }

        /// <summary>
        /// Checks if a specific user is enrolled in the given program.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="programId">The program's unique identifier.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// GeneralResult containing boolean indicating enrollment status.</returns>
        [HttpGet("is-enrolled/")]
        [RequiredPermission(Permissions.ProgramEnrollmentAdminPermissions.IsUserEnrolled)]
        //[SwaggerOperation(Tags = new[] { "ProgramsManegment" })]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IsUserEnrolledForAdmin([FromQuery] string userId, [FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await enrollmentService.IsUserEnrolledAsync(userId, programId, cancellationToken);
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
    }
}
