namespace Lumora.Services.Programs
{
    public class ProgressService(
        IProgressRepository repository, IUserRepository userRepository,
        IUnitOfWork unitOfWork, ILogger<ProgressService> logger,
        IMapper mapper, ProgressMessage messages) : IProgressService
    {
        private readonly IProgressRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ILogger<ProgressService> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly ProgressMessage _messages = messages;

        // Level: Lessons

        /// <inheritdoc/>
        public async Task<GeneralResult> MarkLessonCompletedAsync(int lessonId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validation
                if (lessonId <= 0)
                {
                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Invalid lesson id {LessonId}.", lessonId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. Fetch Entities
                var lesson = await _repository.GetLessonWithProgramAsync(lessonId, cancellationToken);
                if (lesson == null)
                {
                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Lesson {LessonId} not found.", lessonId);
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : User {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var existing = await _repository.GetLessonProgressAsync(userId, lessonId, cancellationToken);

                // 3. Logic Execution
                if (existing != null)
                {
                    if (existing.IsCompleted)
                    {
                        _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Lesson {LessonId} already completed by user {UserId}.", lessonId, userId);
                        return new GeneralResult(true, _messages.MsgLessonAlreadyCompleted, null, ErrorType.Success);
                    }

                    // Update existing record
                    existing.IsCompleted = true;
                    existing.CompletedAt = DateTimeOffset.UtcNow;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;

                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Existing record updated for user {UserId}.", userId);
                }
                else
                {
                    // Create new record
                    var progress = new LessonProgress
                    {
                        UserId = userId,
                        IsCompleted = true,
                        LessonId = lessonId,
                        CompletedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await _repository.AddLessonProgressAsync(progress, cancellationToken);
                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : New record created for user {UserId}.", userId);
                }

                // 4. Persistence (Commit all changes: Update or Add)
                // هذا الاستدعاء يحفظ التعديل في حالة Update أو الإضافة في حالة New
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 5. Trigger Calculations (Updates other progress levels)
                // ملاحظة: هذه الدوال يجب أن تستخدم الـ Repository والـ Unit of Work داخلياً أيضاً
                await UpdateProgramCourseProgressAsync(lesson.ProgramCourseId, userId, cancellationToken);
                await UpdateProgramProgressAsync(lesson.ProgramCourse.ProgramId, userId, cancellationToken);

                return new GeneralResult(true, _messages.MsgLessonCompleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - MarkLessonCompletedAsync : Error marking lesson as completed");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("marking lesson as completed."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<LessonProgressDetailsDto>> GetLessonProgressAsync(string userId, int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                // سحب البيانات عبر المستودع (المستودع هو المسؤول عن الـ Includes والـ NoTracking)
                var data = await _repository.GetLessonProgressDetailsAsync(userId, lessonId, cancellationToken);

                if (data == null)
                {
                    _logger.LogInformation("ProgressService - GetLessonProgressAsync : Lesson {LessonId} progress not found for user {UserId}.", lessonId, userId);
                    return new GeneralResult<LessonProgressDetailsDto>(false, _messages.MsgLessonNotCompleted, null, ErrorType.NotFound);
                }

                // تحويل الكائن إلى DTO (Mapping)
                // ملاحظة: نعتمد هنا على أن الـ Repository قام بتحميل Lesson و ProgramCourse و TrainingProgram
                var lessonProgress = new LessonProgressDetailsDto
                {
                    LessonId = data.LessonId,
                    LessonName = data.Lesson?.Name ?? "N/A",
                    RelatedCourseName = data.Lesson?.ProgramCourse?.Name ?? "N/A",
                    RelatedProgramName = data.Lesson?.ProgramCourse?.TrainingProgram?.Name ?? "N/A",
                    IsCompleted = data.IsCompleted,
                    CompletedAt = data.CompletedAt,
                    TimeSpent = data.TimeSpent
                };

                _logger.LogInformation("ProgressService - GetLessonProgressAsync : Lesson progress retrieved for user {UserId}.", userId);
                return new GeneralResult<LessonProgressDetailsDto>(true, _messages.MsgLessonProgressRetrieved, lessonProgress, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetLessonProgressAsync : Error retrieving lesson progress for user {UserId}", userId);
                return new GeneralResult<LessonProgressDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving lesson progress"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<LessonProgressDetailsDto>>> GetCompletedLessonsAsync(string userId, int courseId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. جلب الكورس مع الدروس والبرنامج التدريبي (لضمان وجود الأسماء)
                var programCourse = await _repository.GetCourseWithLessonsAsync(courseId, cancellationToken);

                if (programCourse == null)
                {
                    _logger.LogInformation("ProgressService - GetCompletedLessonsAsync : Course {CourseId} not found.", courseId);
                    return new GeneralResult<List<LessonProgressDetailsDto>>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                var lessonIds = programCourse.Lessons.Select(l => l.Id).ToList();

                if (!lessonIds.Any())
                {
                    _logger.LogInformation("ProgressService - GetCompletedLessonsAsync : No lessons found for course {CourseId}.", courseId);
                    return new GeneralResult<List<LessonProgressDetailsDto>>(false, _messages.MsgNoLessonsFoundForcourse, null, ErrorType.NotFound);
                }

                // 2. جلب سجلات التقدم للدروس المكتملة فقط
                var progresses = await _repository.GetCompletedLessonsInCourseAsync(userId, lessonIds, cancellationToken);

                if (!progresses.Any())
                {
                    _logger.LogInformation("ProgressService - GetCompletedLessonsAsync : No completed lessons found for user {UserId}.", userId);
                    return new GeneralResult<List<LessonProgressDetailsDto>>(false, _messages.MsgNoCompletedLessons, null, ErrorType.NotFound);
                }

                // 3. Mapping إلى DTO
                var completed = progresses.Select(lp => new LessonProgressDetailsDto
                {
                    LessonId = lp.LessonId,
                    LessonName = lp.Lesson?.Name,
                    RelatedCourseName = programCourse.Name,
                    RelatedProgramName = programCourse.TrainingProgram?.Name,
                    IsCompleted = lp.IsCompleted,
                    CompletedAt = lp.CompletedAt,
                    TimeSpent = lp.TimeSpent
                }).ToList();

                _logger.LogInformation("ProgressService - GetCompletedLessonsAsync : Success for user {UserId}.", userId);
                return new GeneralResult<List<LessonProgressDetailsDto>>(true, _messages.MsgCompletedLessonsRetrieved, completed, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetCompletedLessonsAsync : Error for user {UserId}", userId);
                return new GeneralResult<List<LessonProgressDetailsDto>>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving completed lessons"), null, ErrorType.InternalServerError);
            }
        }

        // Level: Course

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateProgramCourseProgressAsync(int courseId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validation (Fast Fail)
                if (courseId <= 0 || string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - UpdateProgramCourseProgressAsync : Invalid Input. CourseId: {CourseId}, UserId: {UserId}", courseId, userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. Fetch Course with Lessons (Domain Entity)
                var course = await _repository.GetCourseWithLessonsAsync(courseId, cancellationToken);

                if (course == null || course.Lessons == null || !course.Lessons.Any())
                {
                    _logger.LogInformation("ProgressService - UpdateProgramCourseProgressAsync : Course {CourseId} not found or has no lessons.", courseId);
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                // 3. Calculate Progress
                var lessonIds = course.Lessons.Select(l => l.Id).ToList();

                var completedIds = await _repository.GetCompletedLessonIdsAsync(userId, lessonIds, cancellationToken);

                double percentage = (double)completedIds.Count / course.Lessons.Count * 100;
                bool isCompleted = completedIds.Count == course.Lessons.Count;

                // 4. Update or Create TraineeProgress record
                var progressRecord = await _repository.GetTraineeProgressAsync(userId, courseId, null, ProgressLevel.Course, cancellationToken);

                if (progressRecord == null)
                {
                    var newProgress = new TraineeProgress
                    {
                        UserId = userId,
                        CourseId = courseId,
                        CourseType = CourseType.Program,
                        Level = ProgressLevel.Course,
                        CompletionPercentage = percentage,
                        IsCompleted = isCompleted,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await _repository.AddTraineeProgressAsync(newProgress, cancellationToken);
                }
                else
                {
                    // تحديث الحقول فقط في حالة تغيرت البيانات (State Tracking)
                    progressRecord.CompletionPercentage = percentage;
                    progressRecord.IsCompleted = isCompleted;
                    progressRecord.UpdatedAt = DateTimeOffset.UtcNow;
                }

                // 5. Atomic Commit via Unit of Work
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("ProgressService - UpdateProgramCourseProgressAsync : Course progress updated to {Percentage}% for user {UserId}", percentage, userId);

                return new GeneralResult(true, _messages.MsgCourseProgressUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - UpdateProgramCourseProgressAsync : Unexpected error for Course {CourseId}", courseId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating course progress."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<CourseProgressDetailsDto>> GetProgramCourseProgressAsync(string userId, int courseId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validation (Input)
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - GetProgramCourseProgressAsync : Invalid UserId provided.");
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (courseId <= 0)
                {
                    _logger.LogWarning("ProgressService - GetProgramCourseProgressAsync : Invalid CourseId {CourseId} provided.", courseId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. Check User Existence (Infrastructure call via Interface)
                var userExists = await _userRepository.ExistsByIdActiveAsync(userId, cancellationToken);
                if (!userExists)
                {
                    _logger.LogInformation("ProgressService - GetProgramCourseProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // 3. Get Course (Using Repository that handles Includes)
                var course = await _repository.GetCourseWithProgramAsync(courseId, cancellationToken);
                if (course is null)
                {
                    _logger.LogInformation("ProgressService - GetProgramCourseProgressAsync : Program course {CourseId} not found.", courseId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                // 4. Get Progress Data
                var progress = await _repository.GetTraineeProgressAsync(userId, courseId, null, ProgressLevel.Course, cancellationToken);

                if (progress is null)
                {
                    _logger.LogInformation("ProgressService - GetProgramCourseProgressAsync : Progress not found for user {UserId} in course {CourseId}.", userId, courseId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgProgressNotFound, null, ErrorType.NotFound);
                }

                // 5. Mapping to DTO (Application Logic)
                var dto = new CourseProgressDetailsDto
                {
                    CourseId = courseId,
                    CourseName = course.Name,
                    RelatedProgramName = course.TrainingProgram?.Name ?? string.Empty,
                    CompletionPercentage = progress.CompletionPercentage,
                    IsCompleted = progress.IsCompleted,
                    TotalTimeSpent = progress.TotalTimeSpent,
                    CompletedAt = progress.UpdatedAt ?? progress.CreatedAt ?? DateTimeOffset.UtcNow,
                };

                _logger.LogInformation("ProgressService - GetProgramCourseProgressAsync : Course {CourseId} progress retrieved for user {UserId}.", courseId, userId);
                return new GeneralResult<CourseProgressDetailsDto>(true, _messages.MsgCourseProgressRetrieved, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetProgramCourseProgressAsync : Error retrieving course progress for User:{UserId}, Course:{CourseId}", userId, courseId);
                return new GeneralResult<CourseProgressDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving course progress"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<CourseProgressDetailsDto>>> GetUserCoursesProgressAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق من المدخلات (Business Validation)
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - GetUserCoursesProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult<List<CourseProgressDetailsDto>>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. استخدام الـ UserRepository للتحقق من وجود المستخدم (Cross-Service Concern)
                var userExists = await _userRepository.ExistsByIdActiveAsync(userId);
                if (!userExists)
                {
                    _logger.LogInformation("ProgressService - GetUserCoursesProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult<List<CourseProgressDetailsDto>>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // 3. جلب قائمة التقدم من المستودع المخصص
                var progresses = await _repository.GetUserCoursesProgressListAsync(userId, cancellationToken);
                if (progresses == null || !progresses.Any())
                {
                    _logger.LogInformation("ProgressService - GetUserCoursesProgressAsync : No course progress found for user {UserId}.", userId);
                    return new GeneralResult<List<CourseProgressDetailsDto>>(false, _messages.MsgNoCourseProgressFound, null, ErrorType.NotFound);
                }

                var result = new List<CourseProgressDetailsDto>();

                // 4. بناء الـ DTOs (Mapping Logic)
                foreach (var progress in progresses)
                {
                    if (!progress.CourseId.HasValue) continue;

                    string courseName = string.Empty;
                    string? relatedProgramName = null;

                    // تحديد نوع الدورة لجلب البيانات المكملة
                    if (progress.CourseType == CourseType.Program)
                    {
                        var course = await _repository.GetCourseWithProgramAsync(progress.CourseId.Value, cancellationToken);
                        courseName = course?.Name ?? string.Empty;
                        relatedProgramName = course?.TrainingProgram?.Name;
                    }
                    else if (progress.CourseType == CourseType.Live)
                    {
                        var course = await _repository.GetLiveCourseByIdAsync(progress.CourseId.Value, cancellationToken);
                        courseName = course?.Title ?? string.Empty;
                    }

                    result.Add(new CourseProgressDetailsDto
                    {
                        CourseId = progress.CourseId.Value,
                        CourseName = courseName,
                        RelatedProgramName = relatedProgramName,
                        CompletionPercentage = progress.CompletionPercentage,
                        IsCompleted = progress.IsCompleted,
                        TotalTimeSpent = progress.TotalTimeSpent,
                        CompletedAt = progress.CreatedAt ?? progress.CreatedAt ?? DateTimeOffset.UtcNow
                    });
                }

                _logger.LogInformation("ProgressService - GetUserCoursesProgressAsync : User course progresses retrieved for user {UserId}. Count: {Count}", userId, result.Count);
                return new GeneralResult<List<CourseProgressDetailsDto>>(true, _messages.MsgCourseProgressRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetUserCoursesProgressAsync : Error retrieving user course progresses for User {UserId}.", userId);
                return new GeneralResult<List<CourseProgressDetailsDto>>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving user course progresses"), null, ErrorType.InternalServerError);
            }
        }

        // Level: Program

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateProgramProgressAsync(int programId, string userId, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            try
            {
                // 1. التحقق من المدخلات الأساسية
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (programId <= 0)
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid ProgramId {ProgramId} provided.", programId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. التحقق من وجود المستخدم (عبر Repository الهوية/المستخدمين)
                var userExists = await _userRepository.ExistsByIdActiveAsync(userId, cancellationToken);
                if (!userExists)
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // 3. التحقق من وجود البرنامج التدريبي
                var program = await _repository.GetProgramByIdAsync(programId, cancellationToken);
                if (program == null)
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync : Program {ProgramId} not found.", programId);
                    return new GeneralResult(false, _messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                // 4. جلب معرفات الدورات التابعة للبرنامج
                var programCourseIds = await _repository.GetProgramCourseIdsAsync(programId, cancellationToken);
                if (programCourseIds == null || !programCourseIds.Any())
                {
                    return new GeneralResult(false, _messages.MsgNoProgramCoursesFound, null, ErrorType.NotFound);
                }

                // 5. جلب تقدم المستخدم في جميع هذه الدورات (دفعة واحدة - أداء عالٍ)
                var courseProgresses = await _repository.GetCourseProgressesByIdsAsync(userId, programCourseIds, cancellationToken);

                // حساب الإحصائيات بناءً على البيانات المسترجعة
                var totalCoursesCount = programCourseIds.Count;
                var completedCount = courseProgresses.Count(p => p.IsCompleted);

                double percentage = (double)completedCount / totalCoursesCount * 100;
                bool isCompleted = completedCount == totalCoursesCount;

                // 6. تحديث أو إنشاء سجل تقدم البرنامج
                var programProgress = await _repository.GetTraineeProgressAsync(userId, null, programId, ProgressLevel.Program, cancellationToken);

                if (programProgress == null)
                {
                    programProgress = new TraineeProgress
                    {
                        UserId = userId,
                        ProgramId = programId,
                        Level = ProgressLevel.Program,
                        CompletionPercentage = percentage,
                        IsCompleted = isCompleted,
                        CreatedAt = now
                    };
                    await _repository.AddTraineeProgressAsync(programProgress, cancellationToken);
                }
                else
                {
                    programProgress.CompletionPercentage = percentage;
                    programProgress.IsCompleted = isCompleted;
                    programProgress.UpdatedAt = now;
                }

                // 7. تحديث حالة التسجيل (Enrollment) إذا اكتمل البرنامج
                if (isCompleted)
                {
                    var enrollment = await _repository.GetActiveProgramEnrollmentAsync(programId, userId, cancellationToken);
                    if (enrollment != null)
                    {
                        enrollment.EnrollmentStatus = EnrollmentStatus.Completed;
                        enrollment.UpdatedAt = now;
                    }
                }

                // 8. حفظ كافة التغييرات كعملية واحدة (Atomic Transaction)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("ProgressService - UpdateProgramProgressAsync: Program progress updated for user {UserId} in program {ProgramId}.", userId, programId);
                return new GeneralResult(true, _messages.MsgProgramProgressUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - UpdateProgramProgressAsync: Error updating program progress.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating program progress."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<ProgramProgressDetailsDto>> GetProgramProgressAsync(string userId, int programId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق من المدخلات الأساسية
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - GetProgramProgressAsync : Invalid UserId provided.");
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (programId <= 0)
                {
                    _logger.LogWarning("ProgressService - GetProgramProgressAsync : Invalid ProgramId {ProgramId} provided.", programId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. التأكد من وجود المستخدم (عبر مستودع المستخدمين)
                var userExists = await _userRepository.ExistsByIdActiveAsync(userId, cancellationToken);
                if (!userExists)
                {
                    _logger.LogInformation("ProgressService - GetProgramProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // 3. التأكد من وجود البرنامج (عبر مستودع التقدم)
                var program = await _repository.GetProgramByIdAsync(programId, cancellationToken);
                if (program == null)
                {
                    _logger.LogInformation("ProgressService - GetProgramProgressAsync : Program {ProgramId} not found.", programId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                // 4. جلب بيانات التقدم
                var progress = await _repository.GetTraineeProgressAsync(userId, null, programId, ProgressLevel.Program, cancellationToken);

                if (progress == null)
                {
                    _logger.LogInformation("ProgressService - GetProgramProgressAsync: No program progress found for user {UserId} in program {ProgramId}.", userId, programId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgProgramProgressNotFound, null, ErrorType.NotFound);
                }

                // 5. بناء الـ DTO (Mapping)
                // ملاحظة: نعتمد على الكائن 'program' الذي جلبناه في الخطوة 3 لضمان وجود الاسم
                var result = new ProgramProgressDetailsDto
                {
                    ProgramId = programId,
                    ProgramName = program.Name,
                    CompletionPercentage = progress.CompletionPercentage,
                    IsCompleted = progress.IsCompleted,
                    TotalTimeSpent = progress.TotalTimeSpent
                };

                _logger.LogInformation("ProgressService - GetProgramProgressAsync: Program progress retrieved for user {UserId} in program {ProgramId}.", userId, programId);
                return new GeneralResult<ProgramProgressDetailsDto>(true, _messages.MsgProgramProgressRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetProgramProgressAsync: Error retrieving program progress for User: {UserId}, Program: {ProgramId}", userId, programId);
                return new GeneralResult<ProgramProgressDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving program progress."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<ProgramProgressDetailsDto>>> GetUserProgramsProgressAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق من المدخلات
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - GetUserProgramsProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult<List<ProgramProgressDetailsDto>>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. استخدام الـ Repository للتحقق من وجود المستخدم (بدل الاستعلام المباشر)
                var userExists = await _userRepository.ExistsByIdActiveAsync(userId, cancellationToken);
                if (!userExists)
                {
                    _logger.LogInformation("ProgressService - GetUserProgramsProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult<List<ProgramProgressDetailsDto>>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // 3. جلب البيانات من الـ Repository المخصص (تم إزالة _dbContext نهائياً)
                var progresses = await _repository.GetUserProgramsProgressListAsync(userId, cancellationToken);

                if (progresses == null || !progresses.Any())
                {
                    _logger.LogInformation("ProgressService - GetUserProgramsProgressAsync: No program progress found for user {UserId}.", userId);
                    return new GeneralResult<List<ProgramProgressDetailsDto>>(false, _messages.MsgProgramProgressNotFound, null, ErrorType.NotFound);
                }

                // 4. تحويل البيانات (Mapping) إلى DTO
                var result = progresses.Select(p => new ProgramProgressDetailsDto
                {
                    ProgramId = p.ProgramId ?? 0,
                    ProgramName = p.Program?.Name ?? string.Empty,
                    CompletionPercentage = p.CompletionPercentage,
                    IsCompleted = p.IsCompleted,
                    TotalTimeSpent = p.TotalTimeSpent
                }).ToList();

                _logger.LogInformation("ProgressService - GetUserProgramsProgressAsync: Program progresses retrieved for user {UserId}.", userId);

                return new GeneralResult<List<ProgramProgressDetailsDto>>(true, _messages.MsgProgramProgressRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetUserProgramsProgressAsync: Error retrieving progresses.");
                return new GeneralResult<List<ProgramProgressDetailsDto>>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving progresses."), null, ErrorType.InternalServerError);
            }
        }

        // Time Tracking

        /// <inheritdoc/>
        public async Task<GeneralResult> StartLessonSessionAsync(string userId, int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                // 1. التحقق من صحة البيانات المدخلة
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - StartLessonSessionAsync: Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (lessonId <= 0)
                {
                    _logger.LogWarning("ProgressService - StartLessonSessionAsync: Invalid LessonId {LessonId} provided.", lessonId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. التحقق من وجود المستخدم (عبر 
                var userExists = await _userRepository.ExistsByIdActiveAsync(userId, cancellationToken);
                if (!userExists)
                {
                    _logger.LogInformation("ProgressService - StartLessonSessionAsync: User {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // 3. التحقق من وجود الدرس
                var lesson = await _repository.GetLessonWithProgramAsync(lessonId, cancellationToken);
                if (lesson == null)
                {
                    _logger.LogInformation("ProgressService - StartLessonSessionAsync: Lesson {LessonId} not found.", lessonId);
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                // 4.  التحقق من وجود جلسة نشطة
                var existingSession = await _repository.GetActiveLessonSessionAsync(userId, lessonId, cancellationToken);
                if (existingSession != null)
                {
                    _logger.LogInformation("ProgressService - StartLessonSessionAsync: Lesson session already active for user {UserId} and lesson {LessonId}.", userId, lessonId);
                    return new GeneralResult(false, _messages.MsgSessionAlreadyActive, null, ErrorType.BadRequest);
                }

                // 5. إنشاء جلسة جديدة
                var session = new LessonSession
                {
                    UserId = userId,
                    LessonId = lessonId,
                    StartedAt = now
                };

                await _repository.AddLessonSessionAsync(session, cancellationToken);

                // 6. حفظ التغييرات 
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("ProgressService - StartLessonSessionAsync: Started lesson session for user {UserId} and lesson {LessonId}", userId, lessonId);
                return new GeneralResult(true, _messages.MsgSessionStarted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - StartLessonSessionAsync: Error starting lesson session for user {UserId} and lesson {LessonId}", userId, lessonId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("starting lesson session."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> EndLessonSessionAsync(string userId, int lessonId, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            try
            {
                // 1. التحقق من صحة البيانات الأساسية (Basic Validation)
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - EndLessonSessionAsync: Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (lessonId <= 0)
                {
                    _logger.LogWarning("ProgressService - EndLessonSessionAsync: Invalid LessonId {LessonId} provided.", lessonId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. استخدام الـ Repository بدلاً من الـ DbContext (تجنب التلوث)
                // ملاحظة: نحتاج الـ Lesson لمعرفة المدة والـ CourseId
                var lesson = await _repository.GetLessonWithProgramAsync(lessonId, cancellationToken);
                if (lesson == null)
                {
                    _logger.LogInformation("ProgressService - EndLessonSessionAsync: Lesson {LessonId} not found.", lessonId);
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                // 3. البحث عن الجلسة النشطة عبر الـ Repository
                var session = await _repository.GetActiveLessonSessionAsync(userId, lessonId, cancellationToken);
                if (session == null)
                {
                    _logger.LogInformation("ProgressService - EndLessonSessionAsync: No active session found for user {UserId} and lesson {LessonId}.", userId, lessonId);
                    return new GeneralResult(false, _messages.MsgNoActiveSession, null, ErrorType.BadRequest);
                }

                // 4. تنفيذ منطق إغلاق الجلسة وحساب المدة
                session.EndedAt = now;
                var sessionDuration = (session.EndedAt - session.StartedAt) ?? TimeSpan.Zero;

                // 5. تحديث تقدم الدرس (Lesson Progress)
                var lessonProgress = await _repository.GetLessonProgressAsync(userId, lessonId, cancellationToken);

                var totalDurationMinutes = (lessonProgress?.TimeSpent.TotalMinutes ?? 0) + sessionDuration.TotalMinutes;
                var lessonDuration = lesson.DurationInMinutes;
                bool shouldMarkCompleted = lessonDuration > 0 && (totalDurationMinutes / lessonDuration) >= 0.9;

                if (lessonProgress == null)
                {
                    lessonProgress = new LessonProgress
                    {
                        UserId = userId,
                        LessonId = lessonId,
                        TimeSpent = sessionDuration,
                        IsCompleted = shouldMarkCompleted,
                        CompletedAt = shouldMarkCompleted ? now : default,
                        CreatedAt = now
                    };
                    await _repository.AddLessonProgressAsync(lessonProgress, cancellationToken);
                }
                else
                {
                    lessonProgress.TimeSpent += sessionDuration;
                    lessonProgress.UpdatedAt = now;

                    if (shouldMarkCompleted && !lessonProgress.IsCompleted)
                    {
                        lessonProgress.IsCompleted = true;
                        lessonProgress.CompletedAt = now;
                    }
                }

                // 6. حفظ التغييرات الأساسية أولاً (Session & Lesson Progress)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 7. إعادة حساب إجمالي الوقت للدورة والبرنامج (Business Logic)
                var courseId = lesson.ProgramCourseId;

                var totalCourseTicks = await _repository.GetTotalTimeSpentInCourseLessonsAsync(userId, courseId, cancellationToken);
                var courseProgress = await _repository.GetTraineeProgressAsync(userId, courseId, null, ProgressLevel.Course, cancellationToken);

                if (courseProgress != null)
                {
                    courseProgress.TotalTimeSpent = TimeSpan.FromTicks(totalCourseTicks);
                    courseProgress.UpdatedAt = now;
                }

                // حساب إجمالي وقت البرنامج
                if (lesson.ProgramCourse?.ProgramId > 0)
                {
                    var programId = lesson.ProgramCourse.ProgramId;
                    var courseIds = await _repository.GetProgramCourseIdsAsync(programId, cancellationToken);
                    var totalProgramTicks = await _repository.GetTotalTimeSpentInProgramCoursesAsync(userId, courseIds, cancellationToken);

                    var programProgress = await _repository.GetTraineeProgressAsync(userId, null, programId, ProgressLevel.Program, cancellationToken);
                    if (programProgress != null)
                    {
                        programProgress.TotalTimeSpent = TimeSpan.FromTicks(totalProgramTicks);
                        programProgress.UpdatedAt = now;
                    }
                }

                // 8. حفظ النتائج النهائية وتحديث النسب المئوية
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await UpdateProgramCourseProgressAsync(courseId, userId, cancellationToken);

                if (lesson.ProgramCourse?.ProgramId > 0)
                    await UpdateProgramProgressAsync(lesson.ProgramCourse.ProgramId, userId, cancellationToken);

                _logger.LogInformation("ProgressService - EndLessonSessionAsync: Ended lesson session for user {UserId} and lesson {LessonId}", userId, lessonId);
                return new GeneralResult(true, _messages.MsgSessionEnded, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - EndLessonSessionAsync: Error ending lesson session for user {UserId} and lesson {LessonId}", userId, lessonId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("ending lesson session."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SyncAllUserProgressForProgramAsync(int programId, CancellationToken cancellationToken)
        {
            try
            {
                if (programId <= 0)
                {
                    _logger.LogWarning("ProgressService - SyncAllUserProgressForProgramAsync : Invalid ProgramId {ProgramId} provided.", programId);
                    return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                }

                // استخدام الـ Repository بدلاً من استعلام الـ Context المباشر
                var program = await _repository.GetProgramByIdAsync(programId, cancellationToken);

                if (program == null)
                {
                    _logger.LogInformation("ProgressService - SyncAllUserProgressForProgramAsync: Program {ProgramId} not found.", programId);
                    return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                }

                // الحصول على معرفات الكورسات المرتبطة بالبرنامج عبر الـ Repository
                var courseIds = await _repository.GetProgramCourseIdsAsync(programId, cancellationToken);

                if (courseIds == null || !courseIds.Any())
                {
                    _logger.LogInformation("ProgressService - SyncAllUserProgressForProgramAsync: No courses found for Program {ProgramId}.", programId);
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                // الحصول على قائمة المستخدمين المسجلين عبر الـ Repository
                var users = await _repository.GetEnrolledUserIdsInProgramAsync(programId, cancellationToken);

                if (users == null || !users.Any())
                {
                    return new GeneralResult(false, _messages.MsgNoEnrolledUser, null, ErrorType.NotFound);
                }

                foreach (var userId in users)
                {
                    // مزامنة تقدم كل كورس على حدة داخل البرنامج
                    foreach (var courseId in courseIds)
                    {
                        await UpdateProgramCourseProgressAsync(courseId, userId, cancellationToken);
                    }

                    await UpdateProgramProgressAsync(programId, userId, cancellationToken);
                }

                _logger.LogInformation("ProgressService - SyncAllUserProgressForProgramAsync: Synced progress for all users in program {ProgramId}.", programId);
                return new GeneralResult(true, _messages.MsgProgressSynced, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - SyncAllUserProgressForProgramAsync: Error syncing progress for program {ProgramId}.", programId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("syncing progress"), null, ErrorType.InternalServerError);
            }
        }
    }
}
