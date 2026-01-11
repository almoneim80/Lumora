namespace Lumora.Infrastructure.Repositories
{
    public class CourseLessonRepository(PgDbContext dbContext) : ICourseLessonRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<bool> CourseExistsAsync(int courseId, CancellationToken ct)
        {
            return await _dbContext.ProgramCourses
                .AnyAsync(c => c.Id == courseId && !c.IsDeleted, ct);
        }

        public async Task<CourseLesson?> GetByIdAsync(int lessonId, CancellationToken ct)
        {
            return await _dbContext.CourseLessons
                .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, ct);
        }

        public async Task<CourseLesson?> GetLessonWithFullContentAsync(int lessonId, CancellationToken ct)
        {
            return await _dbContext.CourseLessons
                .Include(l => l.LessonAttachments)
                .Include(l => l.LessonTest)
                    .ThenInclude(t => t!.Questions)
                    .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, ct);
        }

        public async Task<List<CourseLesson>> GetLessonsWithContentByCourseIdAsync(int courseId, CancellationToken ct)
        {
            return await _dbContext.CourseLessons.AsNoTracking()
                .Include(l => l.LessonAttachments)
                .Include(l => l.LessonTest)
                    .ThenInclude(e => e!.Questions)
                    .ThenInclude(e => e.Choices)
                .Where(l => l.ProgramCourseId == courseId && !l.IsDeleted)
                .ToListAsync(ct);
        }

        public async Task AddLessonAsync(CourseLesson lesson, CancellationToken ct)
        {
            await _dbContext.CourseLessons.AddAsync(lesson, ct);
        }

        public async Task AddAttachmentsRangeAsync(IEnumerable<LessonAttachment> attachments, CancellationToken ct)
        {
            await _dbContext.LessonAttachments.AddRangeAsync(attachments, ct);
        }

        public async Task AddTestAsync(Test test, CancellationToken ct)
        {
            await _dbContext.Tests.AddAsync(test, ct);
        }
    }
}
