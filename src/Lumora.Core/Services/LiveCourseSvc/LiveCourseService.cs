using Lumora.DTOs.LiveCourse;
using Lumora.Extensions;
using Lumora.Interfaces.LiveCourseIntf;

namespace Lumora.Services.LiveCourseSvc
{
    public class LiveCourseService(
        PgDbContext dbContext, ILogger<LiveCourseService> logger, LiveCourseMessage messages, RoleMessages roleMessages, IRoleService roleService) : ILiveCourseService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly LiveCourseMessage _messages = messages;
        private readonly ILogger<LiveCourseService> _logger = logger;
        private readonly IRoleService _roleService = roleService;

        /// <inheritdoc/>
        public async Task<GeneralResult<int>> CreateAsync(LiveCourseCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var liveCourse = new LiveCourse
                {
                    Title = dto.Title.Trim(),
                    Description = dto.Description.Trim(),
                    Price = dto.Price,
                    ImagePath = dto.ImagePath.Trim(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsActive = true,
                    StudyWay = dto.StudyWay.Trim(),
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Link = dto.Link.Trim(),
                    Lecturer = dto.Lecturer.Trim()
                };

                await _dbContext.LiveCourses.AddAsync(liveCourse, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("LiveCourseService - CreateAsync: Live course created successfully with ID {CourseId}.", liveCourse.Id);
                return new GeneralResult<int>(true, _messages.MsgCourseCreatedSuccessfully, liveCourse.Id, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - CreateAsync: Unexpected error while creating live course.");
                return new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("creating the course"), default, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateAsync(int courseId, LiveCourseUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _dbContext.LiveCourses
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);

                if (course is null)
                {
                    _logger.LogWarning("LiveCourseService - UpdateAsync: Course with ID {CourseId} not found.", courseId);
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                if (!string.IsNullOrWhiteSpace(dto.Title))
                    course.Title = dto.Title.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    course.Description = dto.Description.Trim();

                if (dto.Price.HasValue)
                    course.Price = dto.Price.Value;

                if (!string.IsNullOrWhiteSpace(dto.ImagePath))
                    course.ImagePath = dto.ImagePath.Trim();

                if (!string.IsNullOrWhiteSpace(dto.StudyWay))
                    course.StudyWay = dto.StudyWay.Trim();

                if (dto.StartDate != null)
                    course.StartDate = dto.StartDate;

                if (dto.EndDate != null)
                    course.EndDate = dto.EndDate;

                if (!string.IsNullOrWhiteSpace(dto.Link))
                    course.Link = dto.Link.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Lecturer))
                    course.Lecturer = dto.Lecturer.Trim();

                course.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("LiveCourseService - UpdateAsync: Course with ID {CourseId} updated successfully.", courseId);
                return new GeneralResult(true, _messages.MsgCourseUpdatedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - UpdateAsync: Unexpected error while updating course with ID {CourseId}.", courseId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating the course"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<LiveCourseDetailsDto>> GetByIdAsync(int courseId, CancellationToken cancellationToken)
        {
            var method = nameof(GetByIdAsync);
            try
            {
                if (courseId <= 0)
                {
                    _logger.LogWarning("{Method}: Invalid course ID provided: {CourseId}", method, courseId);
                    return new GeneralResult<LiveCourseDetailsDto>(
                        false, _messages.MsgCourseNotFound, null, ErrorType.Validation);
                }

                var course = await _dbContext.LiveCourses
                    .AsNoTracking()
                    .Include(c => c.UserLiveCourses)
                        .ThenInclude(ulc => ulc.User)
                    .Include(c => c.UserLiveCourses)
                        .ThenInclude(ulc => ulc.PaymentItem)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);

                if (course is null)
                {
                    _logger.LogWarning("LiveCourseService - GetByIdAsync: Course with ID {CourseId} not found.", courseId);
                    return new GeneralResult<LiveCourseDetailsDto>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                var dto = new LiveCourseDetailsDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    Price = course.Price,
                    ImagePath = course.ImagePath,
                    IsActive = course.IsActive,
                    StudyWay = course.StudyWay,
                    Lecturer = course.Lecturer ?? string.Empty,
                    StartDate = course.StartDate ?? DateTimeOffset.UtcNow,
                    EndDate = course.EndDate ?? DateTimeOffset.UtcNow,
                    CreatedAt = course.CreatedAt ?? DateTimeOffset.UtcNow,
                    Link = course.Link,
                    RegisteredUsers = course.UserLiveCourses
                        .Select(x => new UserLiveCourseDto
                        {
                            Id = x.Id,
                            UserId = x.UserId,
                            FullName = x.User.FullName,
                            Email = x.User.Email,
                            RegisteredAt = x.RegisteredAt,
                            PaymentStatus = x.PaymentItem is null ? null : x.PaymentItem.Payment.Status
                        })
                        .ToList()
                };

                _logger.LogInformation("LiveCourseService - GetByIdAsync: Fetched course ID {CourseId} successfully.", courseId);
                return new GeneralResult<LiveCourseDetailsDto>(true, _messages.MsgCourseFetchedSuccessfully, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - GetByIdAsync: Unexpected error while retrieving course ID {CourseId}.", courseId);
                return new GeneralResult<LiveCourseDetailsDto>(false, _messages.GetUnexpectedErrorMessage("retrieving course details"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<LiveCourseListItemDto>>> GetListAsync(
            LiveCourseFilterDto filter, CancellationToken cancellationToken)
        {
            try
            {
                var pagination = new PaginationRequestDto
                {
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                };

                var query = _dbContext.LiveCourses
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted);

                if (filter.IsActive.HasValue)
                    query = query.Where(c => c.IsActive == filter.IsActive.Value);

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                {
                    var keyword = filter.Keyword.Trim();
                    query = query.Where(c =>
                        c.Title.Contains(keyword) ||
                        c.Description.Contains(keyword));
                }

                query = query.OrderByDescending(c => c.CreatedAt);

                var pagedResult = await query
                    .Select(c => new LiveCourseListItemDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Price = c.Price,
                        ImagePath = c.ImagePath,
                        IsActive = c.IsActive,
                        StudyWay = c.StudyWay,
                        StartDate = c.StartDate ?? DateTimeOffset.UtcNow,
                        EndDate = c.EndDate ?? DateTimeOffset.UtcNow,
                        Link = c.Link,
                        Lecturer = c.Lecturer
                    })
                    .ApplyPaginationAsync(pagination, cancellationToken);

                _logger.LogInformation("LiveCourseService - GetListAsync: Retrieved page {PageNumber} with {ItemCount} items.",
                    filter.PageNumber, pagedResult.Items.Count);

                return new GeneralResult<PagedResult<LiveCourseListItemDto>>(
                    true,
                    _messages.MsgCourseListFetchedSuccessfully,
                    pagedResult,
                    ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - GetListAsync: Unexpected error while retrieving course list.");
                return new GeneralResult<PagedResult<LiveCourseListItemDto>>(
                    false,
                    _messages.GetUnexpectedErrorMessage("retrieving course list"),
                    null,
                    ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteAsync(int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _dbContext.LiveCourses
                    .Include(c => c.UserLiveCourses)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);

                if (course is null)
                {
                    _logger.LogWarning("LiveCourseService - DeleteAsync: Course with ID {CourseId} not found.", courseId);
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                if (course.UserLiveCourses.Any())
                {
                    _logger.LogWarning("LiveCourseService - DeleteAsync: Cannot delete course ID {CourseId} with active subscribers.", courseId);
                    return new GeneralResult(false, _messages.MsgCourseHasSubscribers, null, ErrorType.BadRequest);
                }

                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                course.IsDeleted = true;
                course.UpdatedAt = DateTimeOffset.UtcNow;
                course.DeletedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("LiveCourseService - DeleteAsync: Course ID {CourseId} deleted successfully.", courseId);
                return new GeneralResult(true, _messages.MsgCourseDeletedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - DeleteAsync: Unexpected error while deleting course ID {CourseId}.", courseId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("deleting the course"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SetActiveStatusAsync(int courseId, bool isActive, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _dbContext.LiveCourses
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);

                if (course is null)
                {
                    _logger.LogWarning("LiveCourseService - SetActiveStatusAsync: Course with ID {CourseId} not found.", courseId);
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                if (course.IsActive == isActive)
                {
                    var unchangedMsg = isActive ? _messages.MsgCourseAlreadyActive : _messages.MsgCourseAlreadyInactive;
                    _logger.LogInformation("LiveCourseService - SetActiveStatusAsync: Course ID {CourseId} is already in the desired state.", courseId);
                    return new GeneralResult(false, unchangedMsg, null, ErrorType.BadRequest);
                }

                course.IsActive = isActive;
                course.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                var statusMsg = isActive ? _messages.MsgCourseActivated : _messages.MsgCourseDeactivated;
                _logger.LogInformation("LiveCourseService - SetActiveStatusAsync: Course ID {CourseId} status updated to {Status}.", courseId, isActive);
                return new GeneralResult(true, statusMsg, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - SetActiveStatusAsync: Unexpected error while changing course status.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("changing course visibility status"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SubscribeUserAsync(int courseId, string userId, int? paymentItemId, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (paymentItemId is null)
                {
                    _logger.LogWarning("LiveCourseService - SubscribeUserAsync: Payment item ID is required.");
                    return new GeneralResult(false, _messages.MsgPaymentItemRequired, null, ErrorType.BadRequest);
                }

                var course = await _dbContext.LiveCourses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == courseId && !x.IsDeleted, cancellationToken);

                if (course is null)
                {
                    _logger.LogWarning("LiveCourseService - SubscribeUserAsync: Course with ID {CourseId} not found.", courseId);
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                if (user is null)
                {
                    _logger.LogWarning("LiveCourseService - SubscribeUserAsync: User with ID {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var paymentItem = await _dbContext.PaymentItems
                    .Include(p => p.Payment)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.Id == paymentItemId &&
                        p.ItemType == PaymentItemType.LiveCourse &&
                        p.ItemId == courseId &&
                        p.Payment.Status == PaymentStatus.Paid &&
                        p.Payment.UserId == userId,
                        cancellationToken);

                if (paymentItem is null)
                {
                    _logger.LogWarning("LiveCourseService - SubscribeUserAsync: Payment item validation failed for course {CourseId} and user {UserId}.", courseId, userId);
                    return new GeneralResult(false, _messages.MsgInvalidPaymentItem, null, ErrorType.BadRequest);
                }

                var alreadySubscribed = await _dbContext.UserLiveCourses
                    .AsNoTracking()
                    .AnyAsync(x => x.UserId == userId && x.LiveCourseId == courseId, cancellationToken);

                if (alreadySubscribed)
                {
                    _logger.LogInformation("LiveCourseService - SubscribeUserAsync: User {UserId} already enrolled in course {CourseId}.", userId, courseId);
                    return new GeneralResult(false, _messages.MsgAlreadySubscribed, null, ErrorType.BadRequest);
                }

                var subscription = new UserLiveCourse
                {
                    UserId = userId,
                    LiveCourseId = courseId,
                    PaymentItemId = paymentItem.Id,
                    RegisteredAt = DateTimeOffset.UtcNow,
                };

                await _dbContext.UserLiveCourses.AddAsync(subscription, cancellationToken);

                if ((await _roleService.IsUserInRoleAsync(userId, AppRoles.Student, cancellationToken)).Data == false)
                {
                    if ((await _roleService.AssignRoleAsync(userId, AppRoles.Student)).IsSuccess == false)
                    {
                        _logger.LogWarning("RegisterAsync - Failed to assign User role. UserId: {UserId}", userId);
                        await transaction.RollbackAsync(cancellationToken);
                        return new GeneralResult(false, roleMessages.MsgAssignRoleFailed, null, ErrorType.InternalServerError);
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("LiveCourseService - SubscribeUserAsync: User {UserId} successfully enrolled in course {CourseId}.", userId, courseId);
                return new GeneralResult(true, _messages.MsgUserSubscribedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - SubscribeUserAsync: Unexpected error during subscription.");
                await transaction.RollbackAsync(cancellationToken);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("processing the subscription"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<LiveCourseListItemDto>>> GetUserCoursesAsync(string userId, PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var query = _dbContext.UserLiveCourses
                    .AsNoTracking()
                    .Where(x => x.UserId == userId && !x.IsDeleted && x.LiveCourse.IsActive)
                    .OrderByDescending(x => x.RegisteredAt)
                    .Select(x => new LiveCourseListItemDto
                    {
                        Id = x.LiveCourse.Id,
                        Title = x.LiveCourse.Title,
                        Price = x.LiveCourse.Price,
                        ImagePath = x.LiveCourse.ImagePath,
                        IsActive = x.LiveCourse.IsActive,
                        StudyWay = x.LiveCourse.StudyWay,
                        StartDate = x.LiveCourse.StartDate ?? default,
                        EndDate = x.LiveCourse.EndDate ?? default,
                        Link = x.LiveCourse.Link,
                        Lecturer = x.LiveCourse.Lecturer
                    });

                var pagedResult = await query.ApplyPaginationAsync(pagination, cancellationToken);

                _logger.LogInformation("LiveCourseService - GetUserCoursesAsync: Retrieved {Count} live courses for user {UserId}.", pagedResult.Items.Count, userId);

                return new GeneralResult<PagedResult<LiveCourseListItemDto>>(true, _messages.MsgCoursesFetchedSuccessfully, pagedResult, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - GetUserCoursesAsync: Unexpected error while fetching user live courses.");
                return new GeneralResult<PagedResult<LiveCourseListItemDto>>(false, _messages.GetUnexpectedErrorMessage("retrieving live course list"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<UserLiveCourseDto>>> GetCourseSubscribersAsync(
            int courseId, PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var courseExists = await _dbContext.LiveCourses
                    .AsNoTracking()
                    .AnyAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);

                if (!courseExists)
                {
                    _logger.LogWarning("LiveCourseService - GetCourseSubscribersAsync: Course with ID {CourseId} not found.", courseId);
                    return new GeneralResult<PagedResult<UserLiveCourseDto>>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                var query = _dbContext.UserLiveCourses
                    .AsNoTracking()
                    .Where(x => x.LiveCourseId == courseId)
                    .Include(x => x.User)
                    .Include(x => x.PaymentItem)
                        .ThenInclude(p => p == null ? null : p.Payment)
                    .Select(x => new UserLiveCourseDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        FullName = x.User.FullName ?? string.Empty,
                        Email = x.User.Email ?? string.Empty,
                        RegisteredAt = x.RegisteredAt,
                    }).OrderByDescending(x => x.RegisteredAt);

                var pagedResult = await query.ApplyPaginationAsync(pagination, cancellationToken);

                _logger.LogInformation("LiveCourseService - GetCourseSubscribersAsync: Retrieved {Count} subscribers for course ID {CourseId}.",
                    pagedResult.Items.Count, courseId);

                return new GeneralResult<PagedResult<UserLiveCourseDto>>(true, _messages.MsgCourseSubscribersFetchedSuccessfully, pagedResult, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - GetCourseSubscribersAsync: Unexpected error while retrieving paginated subscribers for course ID {CourseId}.", courseId);
                return new GeneralResult<PagedResult<UserLiveCourseDto>>(false, _messages.GetUnexpectedErrorMessage("retrieving course subscribers"), null, ErrorType.InternalServerError);
            }
        }
    }
}
