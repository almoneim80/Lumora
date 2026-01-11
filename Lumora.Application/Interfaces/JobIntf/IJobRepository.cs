namespace Lumora.Application.Interfaces.JobIntf
{
    public interface IJobRepository
    {
        // عمليات الشغل (Job)
        Task<Job?> GetByIdAsync(int jobId, bool track, CancellationToken ct);
        Task<bool> ExistsAsync(int jobId, CancellationToken ct);
        Task AddAsync(Job job, CancellationToken ct);
        void Update(Job job);

        // عمليات طلبات التقديم (Job Applications)
        Task<bool> HasUserAppliedAsync(int jobId, string userId, CancellationToken ct);
        Task AddApplicationAsync(JobApplication application, CancellationToken ct);
        Task<JobApplication?> GetApplicationByIdAsync(int applicationId, CancellationToken ct);
        void UpdateApplication(JobApplication application);

        // عمليات الربط والاسترجاع المعقدة
        Task<List<JobApplication>> GetApplicationsWithDetailsAsync(int? jobId, string? userId, CancellationToken ct);
        Task<bool> IsTraineeEligibleAsync(string userId, CancellationToken ct);
        IQueryable<Job> GetFilteredJobsQuery(JobFilterDto filter);
        Task<List<JobApplication>> GetApplicationsByJobIdAsync(int jobId, CancellationToken ct);
        Task<PagedResult<JobApplicationFullDto>> GetPagedApplicationsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken);
        Task<PagedResult<JobDetailsDto>> GetPagedActiveJobsAsync(JobFilterDto filter, CancellationToken cancellationToken);
    }
}
