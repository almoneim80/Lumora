using Lumora.Application.DTOs.Job;
using Lumora.Application.Interfaces.JobIntf;
using Job = Lumora.Domain.Entities.Tables.Job;

namespace Lumora.Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly PgDbContext _dbContext;

        public JobRepository(PgDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Job?> GetByIdAsync(int jobId, bool track, CancellationToken ct)
        {
            var query = _dbContext.Jobs.AsQueryable();
            if (!track) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(j => j.Id == jobId && !j.IsDeleted, ct);
        }

        public async Task<bool> ExistsAsync(int jobId, CancellationToken ct)
        {
            return await _dbContext.Jobs.AnyAsync(j => j.Id == jobId && !j.IsDeleted, ct);
        }

        public async Task AddAsync(Job job, CancellationToken ct)
        {
            await _dbContext.Jobs.AddAsync(job, ct);
        }

        public void Update(Job job)
        {
            _dbContext.Set<Job>().Update(job);
        }

        public async Task<bool> HasUserAppliedAsync(int jobId, string userId, CancellationToken ct)
        {
            return await _dbContext.JobApplications
                .AnyAsync(a => a.JobId == jobId && a.ApplicantUserId == userId && !a.IsDeleted, ct);
        }

        public async Task AddApplicationAsync(JobApplication application, CancellationToken ct)
        {
            await _dbContext.JobApplications.AddAsync(application, ct);
        }

        public async Task<JobApplication?> GetApplicationByIdAsync(int applicationId, CancellationToken ct)
        {
            return await _dbContext.JobApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);
        }

        public void UpdateApplication(JobApplication application)
        {
            _dbContext.JobApplications.Update(application);
        }

        public async Task<List<JobApplication>> GetApplicationsWithDetailsAsync(int? jobId, string? userId, CancellationToken ct)
        {
            var query = _dbContext.JobApplications
                .Include(a => a.Job)
                .Include(a => a.ApplicantUser)
                .Where(a => !a.IsDeleted);

            if (jobId.HasValue) query = query.Where(a => a.JobId == jobId.Value);
            if (!string.IsNullOrEmpty(userId)) query = query.Where(a => a.ApplicantUserId == userId);

            return await query.ToListAsync(ct);
        }

        public async Task<bool> IsTraineeEligibleAsync(string userId, CancellationToken ct)
        {
            return await _dbContext.TraineeProgresses
                .AnyAsync(p => p.UserId == userId && p.IsCompleted && !p.IsDeleted && p.Program != null, ct);
        }

        public IQueryable<Job> GetFilteredJobsQuery(JobFilterDto filter)
        {
            var query = _dbContext.Jobs.AsNoTracking().Where(j => !j.IsDeleted);

            if (filter.OnlyActive == true) query = query.Where(j => j.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                query = query.Where(j => j.Title.Contains(keyword) || j.Description.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                var location = filter.Location.Trim();
                query = query.Where(j => j.Location.Contains(location));
            }

            if (filter.JobType.HasValue) query = query.Where(j => j.JobType == filter.JobType.Value);

            if (filter.WorkplaceCategory.HasValue) query = query.Where(j => j.WorkplaceCategory == filter.WorkplaceCategory.Value);

            return query.OrderByDescending(j => j.PostedAt);
        }

        public async Task<List<JobApplication>> GetApplicationsByJobIdAsync(int jobId, CancellationToken ct)
        {
            // Retrieve applications with related job and applicant user data
            return await _dbContext.JobApplications
                .Include(a => a.Job)
                .Include(a => a.ApplicantUser)
                .Where(a => a.JobId == jobId && !a.IsDeleted)
                .ToListAsync(ct);
        }

        public async Task<PagedResult<JobApplicationFullDto>> GetPagedApplicationsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            return await _dbContext.JobApplications
                .AsNoTracking()
                .Select(a => new JobApplicationFullDto
                {
                    ApplicationId = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    JobLocation = a.Job.Location,
                    UserId = a.ApplicantUserId,
                    UserFullName = a.ApplicantUser.FullName ?? string.Empty,
                    UserEmail = a.ApplicantUser.Email ?? string.Empty,
                    Status = a.Status,
                    ResumeUrl = a.ResumeUrl,
                    CoverLetter = a.CoverLetter,
                    AppliedAt = a.AppliedAt
                })
                .ApplyPaginationAsync(pagination, cancellationToken);
        }

        public async Task<PagedResult<JobDetailsDto>> GetPagedActiveJobsAsync(JobFilterDto filter, CancellationToken cancellationToken)
        {
            var query = GetFilteredJobsQuery(filter);

            return await query
                .AsNoTracking()
                .Select(job => new JobDetailsDto
                {
                    Id = job.Id,
                    Title = job.Title,
                    Description = job.Description,
                    Location = job.Location,
                    JobType = job.JobType,
                    Salary = job.Salary,
                    Employer = job.Employer,
                    EmployerInfo = job.EmployerInfo,
                    WorkplaceCategory = job.WorkplaceCategory,
                    PostedAt = job.PostedAt,
                    ExpiryDate = job.ExpiryDate,
                    IsActive = job.IsActive
                })
                .ApplyPaginationAsync(filter.Pagination, cancellationToken);
        }
    }
}
