using Lumora.DataAnnotations;
using Lumora.DTOs.Job;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.JobIntf;
namespace Lumora.Web.Controllers.JobAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.JobRoles)]
    public class JobController(
            ILogger<JobController> logger,
            IJobService jobService,
            JobMessages messages) : AuthenticatedController
    {
        /// <summary>
        /// Retrieves a job by its ID.
        /// </summary>
        /// <param name="jobId">The ID of the job.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult containing the job details.</returns>
        [HttpGet("get-one")]
        [RequiredPermission(Permissions.JobPermissions.GetOne)]
        [ProducesResponseType(typeof(GeneralResult<JobDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<JobDetailsDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult<JobDetailsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<JobDetailsDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult<JobDetailsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOne([FromQuery] int jobId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (jobId <= 0)
                {
                    return BadRequest(new GeneralResult<JobDetailsDto>(
                        false, messages.MsgIdInvalid, null, ErrorType.BadRequest));
                }

                var result = await jobService.GetJobByIdAsync(CurrentUserId!, jobId, cancellationToken);
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
                logger.LogError(ex, "JobController - GetOne: Unexpected error while retrieving job with ID {JobId}.", jobId);
                return StatusCode(500,
                    new GeneralResult<JobDetailsDto>(
                        false, messages.GetUnexpectedErrorMessage("retrieving the job"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves all active jobs with optional filters.
        /// </summary>
        /// <param name="filter">Filter criteria such as keyword, location, job type, etc.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult containing a list of job details.</returns>
        [HttpGet("get-all")]
        [RequiredPermission(Permissions.JobPermissions.GetAll)]
        [ProducesResponseType(typeof(GeneralResult<List<JobDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<List<JobDetailsDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult<List<JobDetailsDto>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult<List<JobDetailsDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] JobFilterDto filter, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await jobService.GetAllActiveJobsAsync(CurrentUserId!, filter, cancellationToken);
                if (!result.IsSuccess)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "JobController - GetAll: Unexpected error while retrieving jobs.");
                return StatusCode(500,
                    new GeneralResult<List<JobDetailsDto>>(
                        false, messages.GetUnexpectedErrorMessage("retrieving job list"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Submits a job application for a specific user.
        /// </summary>
        /// <param name="dto">Job application data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult indicating success or failure.</returns>
        [HttpPost("apply")]
        [RequiredPermission(Permissions.JobPermissions.Apply)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Apply([FromBody] JobApplicationCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await jobService.ApplyToJobAsync(CurrentUserId!, dto, cancellationToken);
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
                logger.LogError(ex, "JobController - Apply: Unexpected error while submitting job application.");
                return StatusCode(500,
                    new GeneralResult(false, messages.GetUnexpectedErrorMessage("submitting a job application"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves all job applications submitted by a specific user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult containing list of job applications.</returns>
        [HttpGet("applications/by-user")]
        [RequiredPermission(Permissions.JobPermissions.GetUserApplications)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationDetailsDto>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationDetailsDto>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationDetailsDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserApplications(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await jobService.GetUserApplicationsAsync(CurrentUserId!, cancellationToken);
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
                logger.LogError(ex, "JobController - GetUserApplications: Unexpected error while retrieving user applications.");
                return StatusCode(500,
                    new GeneralResult<List<JobApplicationDetailsDto>>(false, messages.GetUnexpectedErrorMessage("retrieving user applications"), null, ErrorType.InternalServerError));
            }
        }
    }
}
