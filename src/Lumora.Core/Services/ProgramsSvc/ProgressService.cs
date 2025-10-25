using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Services.Programs
{
    public class ProgressService(
        PgDbContext dbContext, ILogger<ProgressService> logger, IMapper mapper, ProgressMessage messages) : IProgressService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<ProgressService> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly ProgressMessage _messages = messages;

        // Level: Lessons

        /// <inheritdoc/>
        public async Task<GeneralResult> MarkLessonCompletedAsync(int lessonId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (lessonId <= 0)
                {
                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Invalid lesson id {LessonId}.", lessonId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var lesson = await _dbContext.CourseLessons
                        .Include(l => l.ProgramCourse).FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, cancellationToken);
                if (lesson == null)
                {
                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Lesson {LessonId} not found.", lessonId);
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                if (user == null)
                {
                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : User {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var existing = await _dbContext.LessonProgresses
                    .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId && !lp.IsDeleted, cancellationToken);

                if (existing != null)
                {
                    if (existing.IsCompleted)
                    {
                        _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Lesson {LessonId} already marked as completed by user {UserId}.", lessonId, userId);
                        return new GeneralResult(true, _messages.MsgLessonAlreadyCompleted, null, ErrorType.Success);
                    }

                    existing.IsCompleted = true;
                    existing.CompletedAt = DateTimeOffset.UtcNow;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;

                    _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Lesson {LessonId} updated to completed for user {UserId}.", lessonId, userId);
                    return new GeneralResult(true, _messages.MsgLessonMarkedAsCompletedNow, null, ErrorType.Success);
                }

                // No existing progress record at all, create one
                var progress = new LessonProgress
                {
                    UserId = userId,
                    IsCompleted = true,
                    LessonId = lessonId,
                    CompletedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _dbContext.LessonProgresses.Add(progress);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // update program course progress.
                await UpdateProgramCourseProgressAsync(lesson.ProgramCourseId, userId, cancellationToken);

                // update program progress.
                await UpdateProgramProgressAsync(lesson.ProgramCourse.ProgramId, userId, cancellationToken);

                _logger.LogInformation("ProgressService - MarkLessonCompletedAsync : Lesson {LessonId} marked as completed by user {UserId}.", lessonId, userId);
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
                var data = await _dbContext.LessonProgresses.AsNoTracking()
                    .Include(lp => lp.Lesson)
                    .ThenInclude(l => l.ProgramCourse)
                    .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId && !lp.IsDeleted, cancellationToken);

                if (data == null)
                {
                    _logger.LogInformation("ProgressService - GetLessonProgressAsync : Lesson {LessonId} not completed by user {UserId}.", lessonId, userId);
                    return new GeneralResult<LessonProgressDetailsDto>(false, _messages.MsgLessonNotCompleted, null, ErrorType.NotFound);
                }

                var lessonProgress = new LessonProgressDetailsDto
                {
                    LessonId = data.LessonId,
                    LessonName = data.Lesson.Name,
                    RelatedCourseName = data.Lesson.ProgramCourse?.Name,
                    RelatedProgramName = data.Lesson.ProgramCourse?.TrainingProgram?.Name,
                    IsCompleted = data.IsCompleted,
                    CompletedAt = data.CompletedAt,
                    TimeSpent = data.TimeSpent
                };

                _logger.LogInformation("ProgressService - GetLessonProgressAsync : Lesson {LessonId} completed by user {UserId}.", lessonId, userId);
                return new GeneralResult<LessonProgressDetailsDto>(true, _messages.MsgLessonProgressRetrieved, lessonProgress, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetLessonProgressAsync : Error retrieving lesson progress");
                return new GeneralResult<LessonProgressDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving lesson progress"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<LessonProgressDetailsDto>>> GetCompletedLessonsAsync(string userId, int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var programCourse = await _dbContext.ProgramCourses
                    .AsNoTracking()
                    .Include(c => c.Lessons)
                    .Include(l => l.TrainingProgram)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);

                if (programCourse == null)
                {
                    _logger.LogInformation("ProgressService - GetCompletedLessonsAsync : No program courses found.");
                    return new GeneralResult<List<LessonProgressDetailsDto>>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                List<int> lessonIds = programCourse.Lessons.Where(l => !l.IsDeleted).Select(l => l.Id).ToList();
                string? relatedCourseName = programCourse.Name;
                string? relatedProgramName = programCourse.TrainingProgram?.Name;

                if (!lessonIds.Any())
                {
                    _logger.LogInformation("ProgressService - GetCompletedLessonsAsync : No lessons found for course {CourseId}.", courseId);
                    return new GeneralResult<List<LessonProgressDetailsDto>>(false, _messages.MsgNoLessonsFoundForcourse, null, ErrorType.NotFound);
                }

                var progresses = await _dbContext.LessonProgresses
                    .AsNoTracking()
                    .Include(lp => lp.Lesson)
                    .ThenInclude(l => l.ProgramCourse)
                    .Where(lp => lp.UserId == userId && lessonIds.Contains(lp.LessonId) && !lp.IsDeleted && lp.IsCompleted)
                    .ToListAsync(cancellationToken);

                if (!progresses.Any())
                {
                    _logger.LogInformation("ProgressService - GetCompletedLessonsAsync : No completed lessons found for user {UserId} in course {CourseId}.", userId, courseId);
                    return new GeneralResult<List<LessonProgressDetailsDto>>(false, _messages.MsgNoCompletedLessons, null, ErrorType.NotFound);
                }

                var completed = progresses.Select(lp => new LessonProgressDetailsDto
                {
                    LessonId = lp.LessonId,
                    LessonName = lp.Lesson.Name,
                    RelatedCourseName = relatedCourseName,
                    RelatedProgramName = relatedProgramName,
                    IsCompleted = lp.IsCompleted,
                    CompletedAt = lp.CompletedAt,
                    TimeSpent = lp.TimeSpent
                }).ToList();

                _logger.LogInformation("ProgressService - GetCompletedLessonsAsync : Completed lessons retrieved for user {UserId} in course {CourseId}.", userId, courseId);
                return new GeneralResult<List<LessonProgressDetailsDto>>(true, _messages.MsgCompletedLessonsRetrieved, completed, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetCompletedLessonsAsync : Error retrieving completed lessons");
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
                if (courseId <= 0)
                {
                    _logger.LogWarning("ProgressService - UpdateProgramCourseProgressAsync : Invalid CourseId {CourseId} provided.", courseId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - UpdateProgramCourseProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var course = await _dbContext.ProgramCourses
                    .AsNoTracking()
                    .Include(c => c.Lessons)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);

                if (course == null || course.Lessons == null || !course.Lessons.Any())
                {
                    _logger.LogInformation("ProgressService - UpdateProgramCourseProgressAsync : Course {CourseId} not found or has no lessons.", courseId);
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                var lessonIds = course.Lessons.Select(l => l.Id).ToList();
                var completedLessonIds = await _dbContext.LessonProgresses
                    .AsNoTracking()
                    .Where(lp => lp.UserId == userId && !lp.IsDeleted && lessonIds.Contains(lp.LessonId))
                    .Select(lp => lp.LessonId)
                    .ToListAsync(cancellationToken);

                if (!completedLessonIds.Any())
                {
                    _logger.LogInformation("ProgressService - UpdateProgramCourseProgressAsync : No completed lessons found for user {UserId} in course {CourseId}.", userId, courseId);
                    return new GeneralResult(false, _messages.MsgNoCompletedLessons, null, ErrorType.NotFound);
                }

                var percentage = (double)completedLessonIds.Count / course.Lessons.Count * 100;
                var isCompleted = completedLessonIds.Count == course.Lessons.Count;

                var result = await _dbContext.TraineeProgresses
                    .FirstOrDefaultAsync(tp =>
                        tp.UserId == userId &&
                        tp.CourseId == courseId &&
                        tp.CourseType == CourseType.Program &&
                        tp.Level == ProgressLevel.Course &&
                        !tp.IsDeleted,
                        cancellationToken);

                if (result == null)
                {
                    var progress = new TraineeProgress
                    {
                        UserId = userId,
                        CourseId = courseId,
                        CourseType = CourseType.Program,
                        Level = ProgressLevel.Course,
                        CompletionPercentage = percentage,
                        IsCompleted = isCompleted,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await _dbContext.TraineeProgresses.AddAsync(progress, cancellationToken);
                }
                else
                {
                    result.CompletionPercentage = percentage;
                    result.IsCompleted = isCompleted;
                    result.UpdatedAt = DateTimeOffset.UtcNow;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("ProgressService - UpdateProgramCourseProgressAsync : Course progress updated for user {UserId} in course {CourseId}.", userId, courseId);

                return new GeneralResult(true, _messages.MsgCourseProgressUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - UpdateProgramCourseProgressAsync : Error updating course progress.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating course progress."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<CourseProgressDetailsDto>> GetProgramCourseProgressAsync(string userId, int courseId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - GetProgramCourseProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (courseId <= 0)
                {
                    _logger.LogWarning("ProgressService - GetProgramCourseProgressAsync : Invalid CourseId {CourseId} provided.", courseId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var userExists = await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);
                if (!userExists)
                {
                    _logger.LogInformation("ProgressService - GetProgramCourseProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var course = await _dbContext.ProgramCourses
                    .AsNoTracking()
                    .Include(c => c.TrainingProgram)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);
                if (course is null)
                {
                    _logger.LogInformation("ProgressService - GetProgramCourseProgressAsync : Program course {CourseId} not found.", courseId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                }

                var progress = await _dbContext.TraineeProgresses.AsNoTracking()
                    .FirstOrDefaultAsync(tp =>
                        tp.UserId == userId &&
                        tp.CourseId == courseId &&
                        tp.CourseType == CourseType.Program &&
                        tp.Level == ProgressLevel.Course &&
                        !tp.IsDeleted, cancellationToken);

                if (progress is null)
                {
                    _logger.LogInformation("ProgressService - GetProgramCourseProgressAsync : Progress not found for user {UserId} in course {CourseId}.", userId, courseId);
                    return new GeneralResult<CourseProgressDetailsDto>(false, _messages.MsgProgressNotFound, null, ErrorType.NotFound);
                }

                var dto = new CourseProgressDetailsDto
                {
                    CourseId = courseId,
                    CourseName = course.Name,
                    RelatedProgramName = course.TrainingProgram?.Name ?? string.Empty,
                    CompletionPercentage = progress.CompletionPercentage,
                    IsCompleted = progress.IsCompleted,
                    TotalTimeSpent = progress.TotalTimeSpent,
                    CompletedAt = progress.CreatedAt ?? DateTimeOffset.UtcNow,
                };

                _logger.LogInformation("ProgressService - GetProgramCourseProgressAsync : Course {CourseId} progress retrieved for user {UserId}.", courseId, userId);
                return new GeneralResult<CourseProgressDetailsDto>(true, _messages.MsgCourseProgressRetrieved, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetProgramCourseProgressAsync : Error retrieving course progress.");
                return new GeneralResult<CourseProgressDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving course progress"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<CourseProgressDetailsDto>>> GetUserCoursesProgressAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - GetUserCoursesProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult<List<CourseProgressDetailsDto>>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var userExists = await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);

                if (!userExists)
                {
                    _logger.LogInformation("ProgressService - GetUserCoursesProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult<List<CourseProgressDetailsDto>>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var progresses = await _dbContext.TraineeProgresses
                    .AsNoTracking()
                    .Where(tp => tp.UserId == userId && tp.CourseId.HasValue && !tp.IsDeleted)
                    .Select(tp => new
                    {
                        tp.CourseId,
                        tp.CourseType,
                        tp.CompletionPercentage,
                        tp.IsCompleted,
                        tp.TotalTimeSpent,
                        tp.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                if (!progresses.Any())
                {
                    _logger.LogInformation("ProgressService - GetUserCoursesProgressAsync : No course progress found for user {UserId}.", userId);
                    return new GeneralResult<List<CourseProgressDetailsDto>>(false, _messages.MsgNoCourseProgressFound, null, ErrorType.NotFound);
                }

                var result = new List<CourseProgressDetailsDto>();

                foreach (var progress in progresses)
                {
                    string courseName = string.Empty;
                    string? relatedProgramName = null;

                    if (progress.CourseType == CourseType.Program)
                    {
                        var course = await _dbContext.ProgramCourses
                            .AsNoTracking()
                            .Include(pc => pc.TrainingProgram)
                            .FirstOrDefaultAsync(c => c.Id == progress.CourseId, cancellationToken);

                        courseName = course?.Name ?? string.Empty;
                        relatedProgramName = course?.TrainingProgram?.Name;
                    }
                    else if (progress.CourseType == CourseType.Live)
                    {
                        var course = await _dbContext.LiveCourses
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == progress.CourseId, cancellationToken);

                        courseName = course?.Title ?? string.Empty;
                    }

                    result.Add(new CourseProgressDetailsDto
                    {
                        CourseId = progress.CourseId!.Value,
                        CourseName = courseName,
                        RelatedProgramName = relatedProgramName,
                        CompletionPercentage = progress.CompletionPercentage,
                        IsCompleted = progress.IsCompleted,
                        TotalTimeSpent = progress.TotalTimeSpent,
                        CompletedAt = progress.CreatedAt ?? DateTimeOffset.UtcNow
                    });
                }

                _logger.LogInformation("ProgressService - GetUserCoursesProgressAsync : User course progresses retrieved for user {UserId}.", userId);
                return new GeneralResult<List<CourseProgressDetailsDto>>(true, _messages.MsgCourseProgressRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetUserCoursesProgressAsync : Error retrieving user course progresses.");
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
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);
                if (!user)
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (programId <= 0)
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid ProgramId {ProgramId} provided.", programId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var program = await _dbContext.TrainingPrograms.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted, cancellationToken);
                if (program == null)
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync : Program {ProgramId} not found.", programId);
                    return new GeneralResult(false, _messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                var programCourses = await _dbContext.ProgramCourses.AsNoTracking()
                    .Where(pc => pc.ProgramId == programId && !pc.IsDeleted)
                    .Select(pc => pc.Id)
                    .ToListAsync(cancellationToken);

                if (!programCourses.Any())
                    return new GeneralResult(false, _messages.MsgNoProgramCoursesFound, null, ErrorType.NotFound);

                var courseProgresses = await _dbContext.TraineeProgresses.AsNoTracking()
                    .Where(tp => tp.UserId == userId &&
                    !tp.IsDeleted && tp.CourseId != null && programCourses.Contains(tp.CourseId.Value))
                    .ToListAsync(cancellationToken);

                if (!courseProgresses.Any())
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync: No course progress found for user {UserId} in program {ProgramId}.", userId, programId);
                    return new GeneralResult(false, _messages.MsgNoCourseProgressFound, null, ErrorType.NotFound);
                }

                var completedCount = courseProgresses.Count(p => p.IsCompleted);
                var percentage = (double)completedCount / programCourses.Count * 100;
                var isCompleted = completedCount == programCourses.Count;

                var result = await _dbContext.TraineeProgresses
                    .FirstOrDefaultAsync(tp => tp.UserId == userId &&
                    tp.ProgramId == programId && tp.Level == ProgressLevel.Program && !tp.IsDeleted, cancellationToken);

                if (result == null)
                {
                    var programProgress = new TraineeProgress
                    {
                        UserId = userId,
                        ProgramId = programId,
                        Level = ProgressLevel.Program,
                        CompletionPercentage = percentage,
                        IsCompleted = isCompleted,
                        CreatedAt = now
                    };

                    await _dbContext.TraineeProgresses.AddAsync(programProgress);
                }
                else
                {
                    result.CompletionPercentage = percentage;
                    result.IsCompleted = isCompleted;
                    result.UpdatedAt = now;
                }

                if (isCompleted)
                {
                    var enrollmentProgramStatus = await _dbContext.ProgramEnrollments
                        .FirstOrDefaultAsync(p => p.ProgramId == programId && p.UserId == userId && !p.IsDeleted
                        && p.EnrollmentStatus == EnrollmentStatus.Active, cancellationToken);
                    if (enrollmentProgramStatus != null)
                    {
                        enrollmentProgramStatus.EnrollmentStatus = EnrollmentStatus.Completed;
                        enrollmentProgramStatus.UpdatedAt = now;
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
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
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);
                if (!user)
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (programId <= 0)
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid ProgramId {ProgramId} provided.", programId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var program = await _dbContext.TrainingPrograms.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted, cancellationToken);
                if (program == null)
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync : Program {ProgramId} not found.", programId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                var progress = await _dbContext.TraineeProgresses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(tp => tp.UserId == userId &&
                    tp.ProgramId == programId && tp.Level == ProgressLevel.Program && !tp.IsDeleted, cancellationToken);

                if (progress == null)
                {
                    _logger.LogInformation("ProgressService - GetProgramProgressAsync: No program progress found for user {UserId} in program {ProgramId}.", userId, programId);
                    return new GeneralResult<ProgramProgressDetailsDto>(false, _messages.MsgProgramProgressNotFound, null, ErrorType.NotFound);
                }

                var result = new ProgramProgressDetailsDto
                {
                    ProgramId = programId,
                    ProgramName = progress.Program != null ? progress.Program.Name : program.Name,
                    CompletionPercentage = progress.CompletionPercentage,
                    IsCompleted = progress.IsCompleted,
                    TotalTimeSpent = progress.TotalTimeSpent
                };

                _logger.LogInformation("ProgressService - GetProgramProgressAsync: Program progress retrieved for user {UserId} in program {ProgramId}.", userId, programId);
                return new GeneralResult<ProgramProgressDetailsDto>(true, _messages.MsgProgramProgressRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - GetProgramProgressAsync: Error retrieving program progress.");
                return new GeneralResult<ProgramProgressDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving program progress."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<ProgramProgressDetailsDto>>> GetUserProgramsProgressAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult<List<ProgramProgressDetailsDto>>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);
                if (!user)
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult<List<ProgramProgressDetailsDto>>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var progresses = await _dbContext.TraineeProgresses
                    .AsNoTracking()
                    .Where(tp => tp.UserId == userId && tp.ProgramId != null &&
                                 tp.Level == ProgressLevel.Program && !tp.IsDeleted).ToListAsync(cancellationToken);

                if (!progresses.Any())
                {
                    _logger.LogInformation("ProgressService - GetUserProgramsProgressAsync: No program progress found for user {UserId}.", userId);
                    return new GeneralResult<List<ProgramProgressDetailsDto>>(false, _messages.MsgProgramProgressNotFound, null, ErrorType.NotFound);
                }

                var result = progresses.Select(p => new ProgramProgressDetailsDto
                {
                    ProgramId = p.ProgramId != null ? p.ProgramId.Value : 0,
                    ProgramName = p.Program != null ? p.Program.Name : string.Empty,
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
                using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);
                if (!user)
                {
                    _logger.LogInformation("ProgressService - UpdateProgramProgressAsync : User {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }


                if (lessonId <= 0)
                {
                    _logger.LogWarning("ProgressService - UpdateProgramProgressAsync : Invalid LessonId {LessonId} provided.", lessonId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var lesson = await _dbContext.CourseLessons.FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, cancellationToken);
                if (lesson == null || lesson.IsDeleted)
                {
                    _logger.LogInformation("ProgressService - StartLessonSessionAsync: Lesson {LessonId} not found.", lessonId);
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                var existingSession = await _dbContext.LessonSessions
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.LessonId == lessonId &&
                    s.EndedAt == null && !s.IsDeleted, cancellationToken);

                if (existingSession != null)
                {
                    _logger.LogInformation("ProgressService - StartLessonSessionAsync: Lesson session already active for user {UserId} and lesson {LessonId}.", userId, lessonId);
                    return new GeneralResult(false, _messages.MsgSessionAlreadyActive, null, ErrorType.BadRequest);
                }

                var session = new LessonSession
                {
                    UserId = userId,
                    LessonId = lessonId,
                    StartedAt = now
                };

                await _dbContext.LessonSessions.AddAsync(session);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("ProgressService - StartLessonSessionAsync: Started lesson session for user {UserId} and lesson {LessonId}", userId, lessonId);
                return new GeneralResult(true, _messages.MsgSessionStarted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgressService - StartLessonSessionAsync: Error starting lesson session for user {UserId} and lesson {LessonId}", userId, lessonId);
                return new GeneralResult(
                    false, _messages.GetUnexpectedErrorMessage("starting lesson session."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> EndLessonSessionAsync(string userId, int lessonId, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ProgressService - EndLessonSessionAsync  : Invalid UserId {UserId} provided.", userId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);
                if (!user)
                {
                    _logger.LogInformation("ProgressService - EndLessonSessionAsync  : User {UserId} not found.", userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                if (lessonId <= 0)
                {
                    _logger.LogWarning("ProgressService - EndLessonSessionAsync  : Invalid LessonId {LessonId} provided.", lessonId);
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var lesson = await _dbContext.CourseLessons.FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, cancellationToken);
                if (lesson == null || lesson.IsDeleted)
                {
                    _logger.LogInformation("ProgressService - EndLessonSessionAsync : Lesson {LessonId} not found.", lessonId);
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                var session = await _dbContext.LessonSessions
                    .Where(s => s.UserId == userId && s.LessonId == lessonId && s.EndedAt == null && !s.IsDeleted)
                    .OrderByDescending(s => s.StartedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (session == null)
                {
                    _logger.LogInformation("ProgressService - EndLessonSessionAsync: No active session found for user {UserId} and lesson {LessonId}.", userId, lessonId);
                    return new GeneralResult(false, _messages.MsgNoActiveSession, null, ErrorType.BadRequest);
                }

                session.EndedAt = now;
                var duration = session.EndedAt - session.StartedAt;
                var sessionDuration = duration ?? TimeSpan.Zero;

                // Update Lesson Progress
                var lessonProgress = await _dbContext.LessonProgresses
                    .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId && !lp.IsDeleted, cancellationToken);

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

                    await _dbContext.LessonProgresses.AddAsync(lessonProgress);
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

                // Recalculate total time spent in Trainee Progress for course.
                var courseId = lesson.ProgramCourseId;
                var courseType = CourseType.Program;
                //var programId = lesson.ProgramCourse?.ProgramId ?? 0;

                var totalCourseTicks = _dbContext.LessonProgresses
                    .Where(lp => lp.UserId == userId && !lp.IsDeleted)
                    .Include(lp => lp.Lesson)
                    .AsEnumerable()
                    .Where(lp => courseType == CourseType.Program && lp.Lesson.ProgramCourseId == courseId)
                    .Sum(lp => lp.TimeSpent.Ticks);

                var courseProgress = await _dbContext.TraineeProgresses
                    .FirstOrDefaultAsync(tp => tp.UserId == userId && tp.CourseId == courseId && tp.CourseType == courseType, cancellationToken);

                if (courseProgress != null)
                {
                    courseProgress.TotalTimeSpent = TimeSpan.FromTicks(totalCourseTicks);
                    courseProgress.UpdatedAt = now;
                }

                // Recalculate total time spent in Trainee Progress for program.
                var programId = await _dbContext.CourseLessons
                    .Where(l => l.Id == lessonId && !l.IsDeleted && l.ProgramCourse != null && l.ProgramCourse.TrainingProgram != null)
                    .Select(l => l.ProgramCourse!.TrainingProgram!.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (programId != 0)
                {
                    var courseIds = await _dbContext.ProgramCourses
                        .Where(pc => pc.ProgramId == programId && !pc.IsDeleted)
                        .Select(pc => pc.Id)
                        .ToListAsync(cancellationToken);

                    var totalTicks = _dbContext.TraineeProgresses
                        .Where(tp => tp.UserId == userId && tp.CourseId.HasValue && courseIds.Contains(tp.CourseId.Value))
                        .AsEnumerable()
                        .Sum(tp => tp.TotalTimeSpent.Ticks);

                    var programProgress = await _dbContext.TraineeProgresses
                        .FirstOrDefaultAsync(tp => tp.UserId == userId && tp.ProgramId == programId, cancellationToken);

                    if (programProgress != null)
                    {
                        programProgress.TotalTimeSpent = TimeSpan.FromTicks(totalTicks);
                        programProgress.UpdatedAt = now;
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                if (courseType == CourseType.Program)
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

                var program = await _dbContext.TrainingPrograms.Include(p => p.ProgramCourses)
                    .ThenInclude(c => c.Lessons).FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted, cancellationToken);

                if (program == null || program.ProgramCourses == null || !program.ProgramCourses.Any())
                {
                    _logger.LogInformation("ProgressService - SyncAllUserProgressForProgramAsync: Program {ProgramId} not found.", programId);
                    return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                }

                var courseIds = program.ProgramCourses.Select(c => c.Id).ToList();
                if (!courseIds.Any()) return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                var users = await _dbContext.ProgramEnrollments
                    .Where(e => e.ProgramId == programId && !e.IsDeleted)
                    .Select(e => e.UserId).Distinct().ToListAsync(cancellationToken);

                if (!users.Any()) return new GeneralResult(false, _messages.MsgNoEnrolledUser, null, ErrorType.NotFound);

                foreach (var userId in users)
                {
                    // Sync each course progress
                    foreach (var courseId in courseIds)
                    {
                        await UpdateProgramCourseProgressAsync(courseId, userId, cancellationToken);
                    }

                    // Sync program progress
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
