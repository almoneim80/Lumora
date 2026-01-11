namespace Lumora.Application.Services.LiveCourseSvc
{
    public class LiveCourseService(
        ILiveCourseRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<LiveCourseService> logger,
        LiveCourseMessage messages,
        RoleMessages roleMessages,
        IRoleService roleService,
        IUserRepository userRepository) : ILiveCourseService
    {
        private readonly ILiveCourseRepository _repository = repository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly RoleMessages _roleMessages = roleMessages;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
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

                await _repository.AddAsync(liveCourse, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
                var course = await _repository.GetByIdAsync(courseId, cancellationToken);

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

                await _unitOfWork.SaveChangesAsync(cancellationToken);

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

                var course = await _repository.GetByIdAsync(courseId, cancellationToken);

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
                // 1. طلب البيانات المفلترة والمقسمة من المستودع
                var (items, totalCount) = await _repository.GetPagedListAsync(
                    filter.IsActive,
                    filter.Keyword,
                    filter.PageNumber,
                    filter.PageSize,
                    cancellationToken);

                // 2. عمل Mapping للنتائج (يدوياً أو عبر AutoMapper)
                var dtos = items.Select(c => new LiveCourseListItemDto
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
                }).ToList();

                // 3. تغليف النتائج في كائن PagedResult
                var pagedResult = new PagedResult<LiveCourseListItemDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                _logger.LogInformation("LiveCourseService - GetListAsync: Retrieved {ItemCount} items.", dtos.Count);

                return new GeneralResult<PagedResult<LiveCourseListItemDto>>(
                    true,
                    _messages.MsgCourseListFetchedSuccessfully,
                    pagedResult,
                    ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService - GetListAsync: Unexpected error.");
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
                var course = await _repository.GetByIdAsync(courseId, cancellationToken);

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

                await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

                course.IsDeleted = true;
                course.UpdatedAt = DateTimeOffset.UtcNow;
                course.DeletedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
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
                var course = await _repository.GetByIdAsync(courseId, cancellationToken);

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

                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                if (paymentItemId is null)
                {
                    _logger.LogWarning("LiveCourseService: Payment item ID is required.");
                    return new GeneralResult(false, _messages.MsgPaymentItemRequired, null, ErrorType.BadRequest);
                }

                // 1. التحقق من وجود الكورس والمستخدم عبر المستودعات المختصة
                var courseExists = await _repository.AnyAsync(courseId, cancellationToken);
                if (!courseExists)
                {
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user is null)
                {
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // 2. التحقق من صحة الدفع
                var isPaymentValid = await _repository.IsPaymentValidAsync(courseId, userId, paymentItemId.Value, cancellationToken);
                if (!isPaymentValid)
                {
                    _logger.LogWarning("LiveCourseService: Payment validation failed for course {CourseId}", courseId);
                    return new GeneralResult(false, _messages.MsgInvalidPaymentItem, null, ErrorType.BadRequest);
                }

                // 3. التحقق من عدم وجود اشتراك مسبق
                var alreadySubscribed = await _repository.IsUserSubscribedAsync(userId, courseId, cancellationToken);
                if (alreadySubscribed)
                {
                    return new GeneralResult(false, _messages.MsgAlreadySubscribed, null, ErrorType.BadRequest);
                }

                // 4. إنشاء سجل الاشتراك
                var subscription = new UserLiveCourse
                {
                    UserId = userId,
                    LiveCourseId = courseId,
                    PaymentItemId = paymentItemId.Value,
                    RegisteredAt = DateTimeOffset.UtcNow,
                };

                await _repository.AddSubscriptionAsync(subscription, cancellationToken);

                // 5. إدارة الصلاحيات (Role Management)
                var roleCheck = await _roleService.IsUserInRoleAsync(userId, AppRoles.Student, cancellationToken);
                if (roleCheck.IsSuccess && roleCheck.Data == false)
                {
                    var assignResult = await _roleService.AssignRoleAsync(userId, AppRoles.Student);
                    if (assignResult.IsSuccess == false)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return new GeneralResult(false, _roleMessages.MsgAssignRoleFailed, null, ErrorType.InternalServerError);
                    }
                }

                // 6. الحفظ النهائي وإتمام المعاملة
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgUserSubscribedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LiveCourseService: Unexpected error during subscription.");
                await transaction.RollbackAsync(cancellationToken);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("processing the subscription"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<LiveCourseListItemDto>>> GetUserCoursesAsync(
    string userId,
    PaginationRequestDto pagination,
    CancellationToken cancellationToken)
        {
            try
            {
                // 1. طلب البيانات المجزأة من المستودع مباشرة
                var pagedEnrollments = await _repository.GetUserEnrollmentsPagedAsync(userId, pagination, cancellationToken);

                // 2.  التحويل إلى DTO (Mapping)
                var dtos = pagedEnrollments.Items.Select(x => new LiveCourseListItemDto
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
                }).ToList();

                // 3. بناء النتيجة النهائية
                var result = new PagedResult<LiveCourseListItemDto>
                {
                    Items = dtos,
                    TotalCount = pagedEnrollments.TotalCount,
                    PageNumber = pagedEnrollments.PageNumber,
                    PageSize = pagedEnrollments.PageSize
                };

                _logger.LogInformation("Retrieved {Count} courses for user {UserId}.", dtos.Count, userId);

                return new GeneralResult<PagedResult<LiveCourseListItemDto>>(
                    true,
                    _messages.MsgCoursesFetchedSuccessfully,
                    result,
                    ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching user live courses for {UserId}.", userId);
                return new GeneralResult<PagedResult<LiveCourseListItemDto>>(false, "Error message", null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<UserLiveCourseDto>>> GetCourseSubscribersAsync(
            int courseId, PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق من وجود الدورة (منطق عمل)
                var courseExists = await _repository.AnyAsync(courseId, cancellationToken);
                if (!courseExists)
                {
                    return new GeneralResult<PagedResult<UserLiveCourseDto>>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                // 2. طلب البيانات المصفحة من المستودع (التنفيذ التقني في Infrastructure)
                var (items, totalCount) = await _repository.GetSubscribersPagedAsync(
                    courseId, pagination.Skip, pagination.PageSize, cancellationToken);

                // 3. تحويل الكيانات إلى DTOs (مسؤولية طبقة التطبيق)
                var dtos = items.Select(x => new UserLiveCourseDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FullName = x.User?.FullName ?? string.Empty,
                    Email = x.User?.Email ?? string.Empty,
                    RegisteredAt = x.RegisteredAt,
                }).ToList();

                var pagedResult = new PagedResult<UserLiveCourseDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize
                };

                return new GeneralResult<PagedResult<UserLiveCourseDto>>(true, _messages.MsgCourseSubscribersFetchedSuccessfully, pagedResult, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscribers for course {CourseId}", courseId);
                return new GeneralResult<PagedResult<UserLiveCourseDto>>(false, _messages.GetUnexpectedErrorMessage("retrieving course subscribers"), null, ErrorType.InternalServerError);
            }
        }
    }
}
