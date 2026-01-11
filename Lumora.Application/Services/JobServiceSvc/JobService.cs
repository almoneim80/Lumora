namespace Lumora.Application.Services.JobServiceSvc
{
    public class JobService(
            IJobRepository jobRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IRoleService roleService,
            ILogger<JobService> logger,
            JobMessages messages) : IJobService
    {
        private readonly IJobRepository _jobRepository = jobRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IRoleService _roleService = roleService;
        private readonly JobMessages _messages = messages;
        private readonly ILogger<JobService> _logger = logger;

        /// <inheritdoc/>
        public async Task<GeneralResult<int>> CreateJobAsync(JobCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Description))
                {
                    _logger.LogWarning("JobService - CreateJobAsync : Title or Description is missing.");
                    return new GeneralResult<int>(false, _messages.MsgMissingTitleOrDescription, default, ErrorType.BadRequest);
                }

                // 2. تحويل الـ DTO إلى Entity
                var job = new Job
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Location = dto.Location,
                    JobType = dto.JobType ?? JobType.FullTime,
                    Salary = dto.Salary,
                    Employer = dto.Employer,
                    EmployerInfo = dto.EmployerInfo,
                    ExpiryDate = dto.ExpiryDate,
                    WorkplaceCategory = dto.WorkplaceCategory ?? WorkplaceCategory.Other,
                    PostedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                // 3. استخدام المستودع للإضافة بدلاً من الـ DbContext
                await _jobRepository.AddAsync(job, cancellationToken);

                // 4. استخدام UnitOfWork لحفظ التغييرات
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("JobService - CreateJobAsync : Job created successfully with ID {JobId}.", job.Id);
                return new GeneralResult<int>(true, _messages.MsgJobCreatedSuccessfully, job.Id, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - CreateJobAsync : Unexpected error occurred while creating job.");
                return new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("creating the job."), default, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateJobAsync(int jobId, JobUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق الأولي من البيانات
                if (jobId <= 0)
                {
                    _logger.LogError("JobService - UpdateJobAsync: Job ID must be greater than 0.");
                    return new GeneralResult(false, _messages.MsgInvalidJobId, null, ErrorType.BadRequest);
                }

                // 2. استخدام المستودع
                var job = await _jobRepository.GetByIdAsync(jobId, track: true, cancellationToken);

                if (job == null)
                {
                    _logger.LogWarning("JobService - UpdateJobAsync: Job with ID {JobId} not found.", jobId);
                    return new GeneralResult(false, _messages.MsgJobNotFound, null, ErrorType.NotFound);
                }

                // 3. تحديث القيم (Mapping logic)
                if (dto.Title is not null) job.Title = dto.Title;
                if (dto.Description is not null) job.Description = dto.Description;
                if (dto.Location is not null) job.Location = dto.Location;
                if (dto.JobType is not null) job.JobType = dto.JobType.Value;
                if (dto.Salary is not null) job.Salary = dto.Salary.Value;
                if (dto.Employer is not null) job.Employer = dto.Employer;
                if (dto.EmployerInfo is not null) job.EmployerInfo = dto.EmployerInfo;
                if (dto.ExpiryDate is not null) job.ExpiryDate = dto.ExpiryDate;
                if (dto.WorkplaceCategory is not null) job.WorkplaceCategory = dto.WorkplaceCategory.Value;
                if (dto.IsActive is not null) job.IsActive = dto.IsActive.Value;

                job.UpdatedAt = DateTimeOffset.UtcNow;

                // 4.  إبلاغ المستودع بالتحديث
                _jobRepository.Update(job);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("JobService - UpdateJobAsync: Job with ID {JobId} updated successfully.", jobId);

                return new GeneralResult(true, _messages.MsgJobUpdatedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - UpdateJobAsync: Unexpected error occurred while updating job.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating the job"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteJobAsync(int jobId, CancellationToken cancellationToken)
        {
            try
            {
                // Use repository to fetch the entity
                var job = await _jobRepository.GetByIdAsync(jobId, track: true, cancellationToken);

                if (job == null)
                {
                    _logger.LogWarning("JobService - DeleteJobAsync: Job with ID {JobId} not found or already deleted.", jobId);
                    return new GeneralResult(false, _messages.MsgJobNotFound, null, ErrorType.NotFound);
                }

                // Apply business logic for soft delete
                job.IsDeleted = true;
                job.DeletedAt = DateTimeOffset.UtcNow;
                job.UpdatedAt = DateTimeOffset.UtcNow;
                job.IsActive = false;

                // Persist changes through Unit of Work
                _jobRepository.Update(job);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("JobService - DeleteJobAsync: Job with ID {JobId} soft-deleted successfully.", jobId);
                return new GeneralResult(true, _messages.MsgJobDeletedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - DeleteJobAsync: Unexpected error occurred while deleting job.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("deleting the job"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<JobDetailsDto>> GetJobByIdAsync(string currentUserId, int jobId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    _logger.LogError("JobService - GetJobByIdAsync: User ID is missing.");
                    return new GeneralResult<JobDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                /* Retrieve user using repository instead of direct context access */
                var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("JobService - GetJobByIdAsync: User with ID {UserId} not found.", currentUserId);
                    return new GeneralResult<JobDetailsDto>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (jobId <= 0)
                {
                    _logger.LogError("JobService - GetJobByIdAsync: Job ID must be greater than 0.");
                    return new GeneralResult<JobDetailsDto>(false, _messages.MsgInvalidJobId, null, ErrorType.BadRequest);
                }

                /* Retrieve job through the specialized JobRepository */
                var job = await _jobRepository.GetByIdAsync(jobId, track: false, cancellationToken);

                if (job == null)
                {
                    _logger.LogWarning("JobService - GetJobByIdAsync: Job with ID {JobId} not found.", jobId);
                    return new GeneralResult<JobDetailsDto>(false, _messages.MsgJobNotFound, null, ErrorType.NotFound);
                }

                var isSuperAdmin = await _roleService.IsUserInRoleAsync(currentUserId, AppRoles.SuperAdmin, cancellationToken);
                if (!isSuperAdmin.Data)
                {
                    if (job.WorkplaceCategory == WorkplaceCategory.Restaurant || job.WorkplaceCategory == WorkplaceCategory.Hotel)
                    {
                        /* Encapsulate eligibility logic within the appropriate repository */
                        var isEligible = await _roleService.IsUserInRoleAsync(currentUserId, AppRoles.SuperAdmin, cancellationToken);

                        if (!isEligible.IsSuccess)
                        {
                            return new GeneralResult<JobDetailsDto>(false, _messages.MsgAccessDenied, null, ErrorType.Forbidden);
                        }
                    }
                }

                var dto = new JobDetailsDto
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
                };

                _logger.LogInformation("JobService - GetJobByIdAsync: Retrieved job with ID {JobId}.", jobId);
                return new GeneralResult<JobDetailsDto>(true, _messages.MsgJobFetchedSuccessfully, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - GetJobByIdAsync: Unexpected error while retrieving job with ID {JobId}.", jobId);
                return new GeneralResult<JobDetailsDto>(false, _messages.GetUnexpectedErrorMessage("retrieving the job"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<JobDetailsDto>>> GetAllActiveJobsAsync(string currentUserId, JobFilterDto filter, CancellationToken cancellationToken)
        {
            try
            {
                /* Validate user identifier */
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    _logger.LogError("JobService - GetAllActiveJobsAsync: User ID is missing.");
                    return new GeneralResult<PagedResult<JobDetailsDto>>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                /* Retrieve user through repository */
                var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("JobService - GetAllActiveJobsAsync: User with ID {UserId} not found.", currentUserId);
                    return new GeneralResult<PagedResult<JobDetailsDto>>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                /* Check authorization and eligibility */
                var isSuperAdmin = await _roleService.IsUserInRoleAsync(currentUserId, AppRoles.SuperAdmin, cancellationToken);
                if (!isSuperAdmin.Data)
                {
                    var isEligible = await _jobRepository.IsTraineeEligibleAsync(currentUserId, cancellationToken);
                    if (!isEligible)
                    {
                        return new GeneralResult<PagedResult<JobDetailsDto>>(false, _messages.MsgAccessDenied, null, ErrorType.Forbidden);
                    }
                }

                /* Project and execute paginated result */
                var pagedResult = await _jobRepository.GetPagedActiveJobsAsync(filter, cancellationToken);

                _logger.LogInformation("JobService - GetAllActiveJobsAsync: Retrieved page {PageNumber} with {ItemCount} jobs.",
                    filter.Pagination.PageNumber, pagedResult.Items.Count);

                return new GeneralResult<PagedResult<JobDetailsDto>>(true, _messages.MsgJobsFetchedSuccessfully, pagedResult, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - GetAllActiveJobsAsync: Unexpected error while retrieving jobs.");
                return new GeneralResult<PagedResult<JobDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("retrieving job list"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ToggleJobActivationAsync(int jobId, CancellationToken cancellationToken)
        {
            try
            {
                /* Retrieve the job through the repository without exposing EF Core logic */
                var job = await _jobRepository.GetByIdAsync(jobId, true, cancellationToken);

                if (job == null)
                {
                    _logger.LogWarning("JobService - ToggleJobActivationAsync: Job with ID {JobId} not found.", jobId);
                    return new GeneralResult(false, _messages.MsgJobNotFound, null, ErrorType.NotFound);
                }

                /* Update business state and metadata */
                job.IsActive = !job.IsActive;
                job.UpdatedAt = DateTimeOffset.UtcNow;

                /* Persist changes through the repository's save method */
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var status = job.IsActive ? _messages.MsgJobActivated : _messages.MsgJobDeactivated;
                _logger.LogInformation("JobService - ToggleJobActivationAsync: Job with ID {JobId} is now {Status}.", jobId, job.IsActive ? "Active" : "Inactive");

                return new GeneralResult(true, status, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - ToggleJobActivationAsync: Unexpected error while toggling job status.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("toggling job activation"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ApplyToJobAsync(string userId, JobApplicationCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                /* Validate input parameters */
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogError("JobService - ApplyToJobAsync: User ID is missing.");
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                /* Check user existence via repository */
                var userExists = await _userRepository.ExistsByIdActiveAsync(userId, cancellationToken);
                if (!userExists)
                {
                    _logger.LogWarning("JobService - ApplyToJobAsync: User with ID {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                /* Check job existence via repository */
                var jobExists = await _jobRepository.ExistsAsync(dto.JobId, cancellationToken);
                if (!jobExists)
                {
                    _logger.LogWarning("JobService - ApplyToJobAsync: Job with ID {JobId} not found.", dto.JobId);
                    return new GeneralResult(false, _messages.MsgJobNotFound, null, ErrorType.NotFound);
                }

                /* Authorization and eligibility check */
                var isSuperAdmin = await _roleService.IsUserInRoleAsync(userId, AppRoles.SuperAdmin, cancellationToken);
                if (!isSuperAdmin.Data)
                {
                    var isEligible = await _jobRepository.IsTraineeEligibleAsync(userId, cancellationToken);
                    if (!isEligible)
                    {
                        return new GeneralResult(false, _messages.MsgAccessDenied, null, ErrorType.Forbidden);
                    }
                }

                /* Prevent duplicate applications */
                var alreadyApplied = await _jobRepository.HasUserAppliedAsync(dto.JobId, userId, cancellationToken);
                if (alreadyApplied)
                {
                    _logger.LogInformation("JobService - ApplyToJobAsync: User {UserId} already applied to job {JobId}.", userId, dto.JobId);
                    return new GeneralResult(false, _messages.MsgAlreadyApplied, null, ErrorType.BadRequest);
                }

                /* Map DTO to domain entity */
                var application = new JobApplication
                {
                    JobId = dto.JobId,
                    ApplicantUserId = userId,
                    CoverLetter = dto.CoverLetter,
                    ResumeUrl = dto.ResumeUrl,
                    Status = JobApplicationStatus.Pending,
                    AppliedAt = DateTimeOffset.UtcNow
                };

                /* Persist application through repository */
                await _jobRepository.AddApplicationAsync(application, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("JobService - ApplyToJobAsync: User {UserId} applied to job {JobId} successfully.", userId, dto.JobId);
                return new GeneralResult(true, _messages.MsgApplicationSubmitted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - ApplyToJobAsync: Unexpected error while applying to job.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("applying to the job"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<JobApplicationDetailsDto>>> GetApplicationsForJobAsync(int jobId, CancellationToken cancellationToken)
        {
            try
            {
                // Check for job existence using repository
                var jobExists = await _jobRepository.ExistsAsync(jobId, cancellationToken);

                if (!jobExists)
                {
                    _logger.LogWarning("JobService - GetApplicationsForJobAsync: Job with ID {JobId} not found.", jobId);
                    return new GeneralResult<List<JobApplicationDetailsDto>>(
                        false,
                        _messages.MsgJobNotFound,
                        null,
                        ErrorType.NotFound);
                }

                // Fetch applications with related data through repository
                var applications = await _jobRepository.GetApplicationsByJobIdAsync(jobId, cancellationToken);

                // Map domain entities to DTOs
                var result = applications.Select(app => new JobApplicationDetailsDto
                {
                    JobDetail = new JobDetailsDto
                    {
                        Id = app.Job.Id,
                        Title = app.Job.Title,
                        Description = app.Job.Description,
                        Location = app.Job.Location,
                        JobType = app.Job.JobType,
                        Salary = app.Job.Salary,
                        Employer = app.Job.Employer,
                        EmployerInfo = app.Job.EmployerInfo,
                        WorkplaceCategory = app.Job.WorkplaceCategory,
                        PostedAt = app.Job.PostedAt,
                        ExpiryDate = app.Job.ExpiryDate,
                        IsActive = app.Job.IsActive
                    },
                    UserProfile = new ApplicantDataDto
                    {
                        FullName = app.ApplicantUser?.FullName,
                        PhoneNumber = app.ApplicantUser?.PhoneNumber,
                        Email = app.ApplicantUser?.Email,
                        City = app.ApplicantUser?.City,
                        Sex = app.ApplicantUser?.Sex,
                        DateOfBirth = app.ApplicantUser?.DateOfBirth ?? default,
                        AboutMe = app.ApplicantUser?.AboutMe,
                        Avatar = app.ApplicantUser?.Avatar
                    }
                }).ToList();

                _logger.LogInformation("JobService - GetApplicationsForJobAsync: Retrieved {Count} applications for job {JobId}.", result.Count, jobId);

                return new GeneralResult<List<JobApplicationDetailsDto>>(true, _messages.MsgApplicationsFetchedSuccessfully, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - GetApplicationsForJobAsync: Unexpected error while retrieving job applications.");
                return new GeneralResult<List<JobApplicationDetailsDto>>(
                    false,
                    _messages.GetUnexpectedErrorMessage("retrieving job applications"),
                    null,
                    ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<JobApplicationDetailsDto>>> GetUserApplicationsAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user exists through the repository
                var userExists = await _userRepository.ExistsByIdActiveAsync(userId, cancellationToken);

                if (!userExists)
                {
                    _logger.LogWarning("JobService - GetUserApplicationsAsync: User with ID {UserId} not found.", userId);
                    return new GeneralResult<List<JobApplicationDetailsDto>>(
                        false,
                        _messages.MsgUserNotFound,
                        null,
                        ErrorType.NotFound);
                }

                // Retrieve applications with included details from repository
                var applications = await _jobRepository.GetApplicationsWithDetailsAsync(null, userId, cancellationToken);

                // Map domain entities to DTOs
                var result = applications.Select(app => new JobApplicationDetailsDto
                {
                    JobDetail = new JobDetailsDto
                    {
                        Id = app.Job.Id,
                        Title = app.Job.Title,
                        Description = app.Job.Description,
                        Location = app.Job.Location,
                        JobType = app.Job.JobType,
                        Salary = app.Job.Salary,
                        Employer = app.Job.Employer,
                        EmployerInfo = app.Job.EmployerInfo,
                        WorkplaceCategory = app.Job.WorkplaceCategory,
                        PostedAt = app.Job.PostedAt,
                        ExpiryDate = app.Job.ExpiryDate,
                        IsActive = app.Job.IsActive
                    },
                    UserProfile = new ApplicantDataDto
                    {
                        FullName = app.ApplicantUser.FullName,
                        PhoneNumber = app.ApplicantUser.PhoneNumber,
                        Email = app.ApplicantUser.Email,
                        City = app.ApplicantUser.City,
                        Sex = app.ApplicantUser.Sex,
                        DateOfBirth = app.ApplicantUser.DateOfBirth ?? default,
                        AboutMe = app.ApplicantUser.AboutMe,
                        Avatar = app.ApplicantUser.Avatar
                    }
                }).ToList();

                _logger.LogInformation("JobService - GetUserApplicationsAsync: Retrieved {Count} applications for user {UserId}.", result.Count, userId);
                return new GeneralResult<List<JobApplicationDetailsDto>>(true, _messages.MsgApplicationsFetchedSuccessfully, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - GetUserApplicationsAsync: Unexpected error while retrieving user applications.");
                return new GeneralResult<List<JobApplicationDetailsDto>>(
                    false,
                    _messages.GetUnexpectedErrorMessage("retrieving user applications"),
                    null,
                    ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateApplicationStatusAsync(int applicationId, JobApplicationStatus newStatus, CancellationToken cancellationToken)
        {
            try
            {
                /* Fetch application through repository abstraction */
                var application = await _jobRepository.GetApplicationByIdAsync(applicationId, cancellationToken);

                if (application is null)
                {
                    _logger.LogWarning("JobService - UpdateApplicationStatusAsync: Application with ID {ApplicationId} not found.", applicationId);
                    return new GeneralResult(false, _messages.GetApplicationNotFoundMessage(applicationId), null, ErrorType.NotFound);
                }

                if (application.Status == newStatus)
                {
                    _logger.LogInformation("JobService - UpdateApplicationStatusAsync: Status is already set to {Status} for application {ApplicationId}.", newStatus, applicationId);
                    return new GeneralResult(false, _messages.GetSameStatusMessage(newStatus.ToString()), null, ErrorType.BadRequest);
                }

                application.Status = newStatus;
                application.UpdatedAt = DateTimeOffset.UtcNow;

                /* Apply changes via repository methods */
                _jobRepository.UpdateApplication(application);

                /* Persist changes through repository abstraction layer */
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("JobService - UpdateApplicationStatusAsync: Status updated to {Status} for application {ApplicationId}.", newStatus, applicationId);
                return new GeneralResult(true, _messages.MsgApplicationStatusUpdated, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - UpdateApplicationStatusAsync: Error while updating application status.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating application status"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<JobApplicationFullDto>>> GetAllApplicationsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                /* Accessing the data through repository queryable to keep Application Layer clean */
                var pagedResult = await _jobRepository.GetPagedApplicationsAsync(pagination, cancellationToken);

                if (!pagedResult.Items.Any())
                {
                    _logger.LogInformation("JobService - GetAllApplicationsAsync: No job applications found.");
                    return new GeneralResult<PagedResult<JobApplicationFullDto>>(false, _messages.MsgNoApplicationsFound, pagedResult);
                }

                _logger.LogInformation("JobService - GetAllApplicationsAsync: Retrieved {ItemCount} job applications on page {PageNumber}.",
                    pagedResult.Items.Count, pagination.PageNumber);

                return new GeneralResult<PagedResult<JobApplicationFullDto>>(true, _messages.MsgApplicationsRetrieved, pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobService - GetAllApplicationsAsync: Error while retrieving applications.");
                return new GeneralResult<PagedResult<JobApplicationFullDto>>(false, _messages.GetUnexpectedErrorMessage("retrieving job applications"), null, ErrorType.InternalServerError);
            }
        }
    }
}
