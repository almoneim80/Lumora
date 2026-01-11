namespace Lumora.Infrastructure.Repositories
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly PgDbContext _dbContext;

        public ProgressRepository(PgDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // --- Lesson Operations ---

        /// <inheritdoc/>
        public async Task<CourseLesson?> GetLessonWithProgramAsync(int lessonId, CancellationToken cancellationToken)
        {
            return await _dbContext.CourseLessons
                .Include(l => l.ProgramCourse)
                .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<LessonProgress?> GetLessonProgressAsync(string userId, int lessonId, CancellationToken cancellationToken)
        {
            // نستخدم هذا للاستعلامات التي تحتاج تحديث (Tracking)
            return await _dbContext.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.UserId == userId &&
                                          lp.LessonId == lessonId &&
                                          !lp.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<LessonProgress?> GetLessonProgressDetailsAsync(string userId, int lessonId, CancellationToken cancellationToken)
        {
            // نستخدم AsNoTracking لسرعة الأداء في عمليات العرض فقط
            return await _dbContext.LessonProgresses
                .AsNoTracking()
                .Include(lp => lp.Lesson)
                    .ThenInclude(l => l.ProgramCourse)
                        .ThenInclude(p => p.TrainingProgram)
                .FirstOrDefaultAsync(lp => lp.UserId == userId &&
                                          lp.LessonId == lessonId &&
                                          !lp.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<LessonProgress>> GetCompletedLessonsInCourseAsync(string userId, List<int> lessonIds, CancellationToken cancellationToken)
        {
            return await _dbContext.LessonProgresses
                .AsNoTracking()
                .Include(lp => lp.Lesson)
                    .ThenInclude(l => l.ProgramCourse)
                .Where(lp => lp.UserId == userId &&
                             lessonIds.Contains(lp.LessonId) &&
                             !lp.IsDeleted &&
                             lp.IsCompleted)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task AddLessonProgressAsync(LessonProgress progress, CancellationToken cancellationToken)
        {
            await _dbContext.LessonProgresses.AddAsync(progress, cancellationToken);
            // ملاحظة للمهندس: SaveChangesAsync سيتم استدعاؤها في الـ Service 
            // للحفاظ على الـ Transaction إذا كان هناك عمليات أخرى.
        }


        // --- Course Operations ---

        /// <inheritdoc/>
        public async Task<ProgramCourse?> GetCourseWithLessonsAsync(int courseId, CancellationToken cancellationToken)
        {
            return await _dbContext.ProgramCourses
                .AsNoTracking()
                .Include(c => c.Lessons.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ProgramCourse?> GetCourseWithProgramAsync(int courseId, CancellationToken cancellationToken)
        {
            return await _dbContext.ProgramCourses
                .AsNoTracking()
                .Include(c => c.TrainingProgram)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<int>> GetCompletedLessonIdsAsync(string userId, List<int> lessonIds, CancellationToken cancellationToken)
        {
            return await _dbContext.LessonProgresses
                .AsNoTracking()
                .Where(lp => lp.UserId == userId &&
                             lessonIds.Contains(lp.LessonId) &&
                             lp.IsCompleted &&
                             !lp.IsDeleted)
                .Select(lp => lp.LessonId)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TraineeProgress?> GetTraineeProgressAsync(string userId, int? courseId, int? programId, ProgressLevel level, CancellationToken cancellationToken)
        {
            var query = _dbContext.TraineeProgresses.Where(tp => tp.UserId == userId &&
                                                               tp.Level == level &&
                                                               !tp.IsDeleted);

            if (level == ProgressLevel.Course && courseId.HasValue)
            {
                query = query.Where(tp => tp.CourseId == courseId.Value && tp.CourseType == CourseType.Program);
            }
            else if (level == ProgressLevel.Program && programId.HasValue)
            {
                query = query.Where(tp => tp.ProgramId == programId.Value);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<TraineeProgress>> GetUserCoursesProgressListAsync(string userId, CancellationToken cancellationToken)
        {
            return await _dbContext.TraineeProgresses
                .AsNoTracking()
                .Where(tp => tp.UserId == userId &&
                             tp.CourseId.HasValue &&
                             !tp.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        // --- Program & Enrollment Operations ---

        /// <inheritdoc/>
        public async Task<TrainingProgram?> GetProgramByIdAsync(int programId, CancellationToken cancellationToken)
        {
            return await _dbContext.TrainingPrograms
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<int>> GetProgramCourseIdsAsync(int programId, CancellationToken cancellationToken)
        {
            return await _dbContext.ProgramCourses
                .AsNoTracking()
                .Where(pc => pc.ProgramId == programId && !pc.IsDeleted)
                .Select(pc => pc.Id)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<TraineeProgress>> GetCourseProgressesInProgramAsync(string userId, List<int> programCourseIds, CancellationToken cancellationToken)
        {
            return await _dbContext.TraineeProgresses
                .AsNoTracking()
                .Where(tp => tp.UserId == userId &&
                             !tp.IsDeleted &&
                             tp.CourseId.HasValue &&
                             programCourseIds.Contains(tp.CourseId.Value))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ProgramEnrollment?> GetActiveProgramEnrollmentAsync(int programId, string userId, CancellationToken cancellationToken)
        {
            return await _dbContext.ProgramEnrollments
                .FirstOrDefaultAsync(p => p.ProgramId == programId &&
                                         p.UserId == userId &&
                                         !p.IsDeleted &&
                                         p.EnrollmentStatus == EnrollmentStatus.Active, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<TraineeProgress>> GetUserProgramsProgressListAsync(string userId, CancellationToken cancellationToken)
        {
            return await _dbContext.TraineeProgresses
                .AsNoTracking()
                .Include(tp => tp.Program) // نحتاج الـ Include لجلب اسم البرنامج
                .Where(tp => tp.UserId == userId &&
                             tp.ProgramId != null &&
                             tp.Level == ProgressLevel.Program &&
                             !tp.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        // --- Shared & General Operations ---

        /// <inheritdoc/>
        public async Task<LiveCourse?> GetLiveCourseByIdAsync(int courseId, CancellationToken cancellationToken)
        {
            return await _dbContext.LiveCourses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task AddTraineeProgressAsync(TraineeProgress progress, CancellationToken cancellationToken)
        {
            await _dbContext.TraineeProgresses.AddAsync(progress, cancellationToken);
        }

        public async Task<List<TraineeProgress>> GetCourseProgressesByIdsAsync(string userId, List<int> courseIds, CancellationToken cancellationToken)
        {
            return await _dbContext.TraineeProgresses
                .Where(p => p.UserId == userId &&
                            p.CourseId.HasValue &&
                            courseIds.Contains(p.CourseId.Value) &&
                            !p.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<LessonSession?> GetActiveLessonSessionAsync(string userId, int lessonId, CancellationToken cancellationToken)
        {
            return await _dbContext.LessonSessions
                .Where(s => s.UserId == userId &&
                            s.LessonId == lessonId &&
                            s.EndedAt == null &&
                            !s.IsDeleted)
                .OrderByDescending(s => s.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddLessonSessionAsync(LessonSession session, CancellationToken cancellationToken)
        {
            await _dbContext.LessonSessions.AddAsync(session, cancellationToken);
        }

        public async Task<long> GetTotalTimeSpentInCourseLessonsAsync(string userId, int courseId, CancellationToken cancellationToken)
        {
            // ملاحظة: تم استخدام Sum على Ticks لضمان الدقة والأداء داخل قاعدة البيانات
            return await _dbContext.LessonProgresses
                .Where(lp => lp.UserId == userId &&
                            lp.Lesson.ProgramCourseId == courseId &&
                            !lp.IsDeleted)
                .SumAsync(lp => lp.TimeSpent.Ticks, cancellationToken);
        }

        public async Task<long> GetTotalTimeSpentInProgramCoursesAsync(string userId, List<int> courseIds, CancellationToken cancellationToken)
        {
            return await _dbContext.TraineeProgresses
                .Where(tp => tp.UserId == userId &&
                            tp.CourseId.HasValue &&
                            courseIds.Contains(tp.CourseId.Value) &&
                            !tp.IsDeleted)
                .SumAsync(tp => tp.TotalTimeSpent.Ticks, cancellationToken);
        }

        public async Task<List<string>> GetEnrolledUserIdsInProgramAsync(int programId, CancellationToken cancellationToken)
        {
            return await _dbContext.ProgramEnrollments
                .Where(e => e.ProgramId == programId && !e.IsDeleted)
                .Select(e => e.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
