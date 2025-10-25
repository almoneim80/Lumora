using Lumora.DataAnnotations;
using Lumora.DTOs.Job;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.JobIntf;
using static Lumora.Infrastructure.PermissionInfra.Permissions;
namespace Lumora.Web.Controllers.JobAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class AdminJobController(
            ILogger<AdminJobController> logger,
            IJobService jobService,
            IExtendedBaseService extendedBaseService,
            ILocalizationManager localization,
            JobMessages messages) : AuthenticatedController
    {
        /// <summary>
        /// Creates a new job posting.
        /// </summary>
        /// <param name="dto">Job creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult containing the new job ID.</returns>
        [HttpPost("create")]
        [RequiredPermission(JobAdminPermissions.Create)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(JobCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await jobService.CreateJobAsync(dto, cancellationToken);
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
                logger.LogError(ex, "JobController - Create: Unexpected error while creating a job.");
                return StatusCode(500,
                    new GeneralResult<int>(false, messages.GetUnexpectedErrorMessage("creating a job"), default, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Updates an existing job posting.
        /// </summary>
        /// <param name="jobId">The ID of the job to update.</param>
        /// <param name="dto">Job update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult indicating success or failure.</returns>
        [HttpPatch("update")]
        [RequiredPermission(JobAdminPermissions.Update)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromQuery] int jobId, [FromBody] JobUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await jobService.UpdateJobAsync(jobId, dto, cancellationToken);
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
                logger.LogError(ex, "JobController - Update: Unexpected error while updating job with ID {JobId}.", jobId);
                return StatusCode(500,
                    new GeneralResult(false, messages.GetUnexpectedErrorMessage("updating the job"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Soft deletes a job by its ID.
        /// </summary>
        /// <param name="jobId">The ID of the job to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult indicating success or failure.</returns>
        [HttpDelete("delete")]
        [RequiredPermission(JobAdminPermissions.Delete)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] int jobId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (jobId <= 0)
                {
                    return BadRequest(new GeneralResult(false, messages.MsgIdInvalid, null, ErrorType.BadRequest));
                }

                var result = await jobService.DeleteJobAsync(jobId, cancellationToken);
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
                logger.LogError(ex, "JobController - Delete: Unexpected error while deleting job with ID {JobId}.", jobId);
                return StatusCode(500,
                    new GeneralResult(false, messages.GetUnexpectedErrorMessage("deleting the job"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Toggles the activation status (active/inactive) of a specific job.
        /// </summary>
        /// <param name="jobId">ID of the job to toggle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult indicating success or failure.</returns>
        [HttpPatch("toggle-activation")]
        [RequiredPermission(JobAdminPermissions.ToggleActivation)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleActivation([FromQuery] int jobId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await jobService.ToggleJobActivationAsync(jobId, cancellationToken);
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
                logger.LogError(ex, "JobController - ToggleActivation: Unexpected error while toggling job activation.");
                return StatusCode(500,
                    new GeneralResult(false, messages.GetUnexpectedErrorMessage("toggling job activation"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Updates the status of a job application (e.g., Accepted, Rejected).
        /// </summary>
        /// <param name="applicationId">Application ID to update.</param>
        /// <param name="newStatus">New status to apply (e.g., Accepted, Rejected).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult indicating success or failure.</returns>
        [HttpPatch("applications/update-status")]
        [RequiredPermission(JobAdminPermissions.UpdateApplicationStatus)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateApplicationStatus([FromQuery] int applicationId, [FromQuery] JobApplicationStatus newStatus, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await jobService.UpdateApplicationStatusAsync(applicationId, newStatus, cancellationToken);
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
                logger.LogError(ex, "JobController - UpdateApplicationStatus: Unexpected error while updating application status.");
                return StatusCode(500,
                    new GeneralResult(false, messages.GetUnexpectedErrorMessage("updating application status"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves all job applications with job and user details (admin view).
        /// </summary>
        /// <param name="pagination">Pagination parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult containing a list of applications with full details.</returns>
        [HttpGet("applications/all")]
        [RequiredPermission(JobAdminPermissions.GetAllApplications)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationFullDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationFullDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationFullDto>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationFullDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllApplications(PaginationRequestDto pagination,  CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await jobService.GetAllApplicationsAsync(pagination, cancellationToken);
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
                logger.LogError(ex, "JobController - GetAllApplications: Unexpected error while retrieving all job applications.");
                return StatusCode(500,
                    new GeneralResult<List<JobApplicationFullDto>>(false, messages.GetUnexpectedErrorMessage("retrieving all job applications"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves all applications submitted for a specific job.
        /// </summary>
        /// <param name="jobId">ID of the job.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ActionResult with GeneralResult containing list of job applications.</returns>
        [HttpGet("applications/by-job")]
        [RequiredPermission(JobAdminPermissions.GetApplicationsByJob)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationDetailsDto>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationDetailsDto>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult<List<JobApplicationDetailsDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetApplicationsByJob([FromQuery] int jobId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await jobService.GetApplicationsForJobAsync(jobId, cancellationToken);
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
                logger.LogError(ex, "JobController - GetApplicationsByJob: Unexpected error while retrieving applications.");
                return StatusCode(500,
                    new GeneralResult<List<JobApplicationDetailsDto>>(false, messages.GetUnexpectedErrorMessage("retrieving job applications"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves all job types.
        /// </summary>
        [HttpGet("job-type")]
        [ProducesResponseType(typeof(IEnumerable<EnumData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        [RequiredPermission(Permissions.StaticContentPermissions.GetStaticContentType)]
        public ActionResult<IEnumerable<EnumData>> GetJobType()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new GeneralResult { IsSuccess = false, Message = localization.GetLocalizedString("UserNotLoggedIn") });
                }

                var enumValues = extendedBaseService.GetEnumValues<JobType>();
                return Ok(enumValues);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred in the static content GetJobType method.");
                return this.UnexpectedError("getting job types.");
            }
        }

        /// <summary>
        /// Retrieves all workplace categories.
        /// </summary>
        [HttpGet("workplace-Category")]
        [ProducesResponseType(typeof(IEnumerable<EnumData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        [RequiredPermission(Permissions.StaticContentPermissions.GetStaticContentType)]
        public ActionResult<IEnumerable<EnumData>> GetWorkplaceCategory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new GeneralResult { IsSuccess = false, Message = localization.GetLocalizedString("UserNotLoggedIn") });
                }

                var enumValues = extendedBaseService.GetEnumValues<WorkplaceCategory>();
                return Ok(enumValues);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred in the static content GetWorkplaceCategory method.");
                return this.UnexpectedError("getting all workplace categories.");
            }
        }
    }
}
