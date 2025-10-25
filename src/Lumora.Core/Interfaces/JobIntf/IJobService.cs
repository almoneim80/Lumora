using Lumora.DTOs.Job;

namespace Lumora.Interfaces.JobIntf
{
    public interface IJobService
    {
        // --- Jobs Management ---

        /// <summary>
        /// Create a new job posting.
        /// </summary>
        Task<GeneralResult<int>> CreateJobAsync(JobCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Update an existing job.
        /// </summary>
        Task<GeneralResult> UpdateJobAsync(int jobId, JobUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Soft delete a job by ID.
        /// </summary>
        Task<GeneralResult> DeleteJobAsync(int jobId, CancellationToken cancellationToken);

        /// <summary>
        /// Get job details by ID.
        /// </summary>
        Task<GeneralResult<JobDetailsDto>> GetJobByIdAsync(string currentUserId, int jobId, CancellationToken cancellationToken);

        /// <summary>
        /// Get all active jobs (optionally with filters).
        /// </summary>
        Task<GeneralResult<PagedResult<JobDetailsDto>>> GetAllActiveJobsAsync(string currentUserId, JobFilterDto filter, CancellationToken cancellationToken);

        /// <summary>
        /// Toggle job activation status (active/inactive).
        /// </summary>
        Task<GeneralResult> ToggleJobActivationAsync(int jobId, CancellationToken cancellationToken);

        // --- Applications Management ---

        /// <summary>
        /// Submit an application to a job.
        /// </summary>
        Task<GeneralResult> ApplyToJobAsync(string userId, JobApplicationCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Get all applications for a specific job.
        /// </summary>
        Task<GeneralResult<List<JobApplicationDetailsDto>>> GetApplicationsForJobAsync(int jobId, CancellationToken cancellationToken);

        /// <summary>
        /// Get applications submitted by a specific user.
        /// </summary>
        Task<GeneralResult<List<JobApplicationDetailsDto>>> GetUserApplicationsAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Update the status of a job application (e.g., Accepted, Rejected).
        /// </summary>
        Task<GeneralResult> UpdateApplicationStatusAsync(int applicationId, JobApplicationStatus newStatus, CancellationToken cancellationToken);

        /// <summary>
        /// Get all job applications with job and user details (admin view).
        /// </summary>
        Task<GeneralResult<PagedResult<JobApplicationFullDto>>> GetAllApplicationsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken);
    }
}
