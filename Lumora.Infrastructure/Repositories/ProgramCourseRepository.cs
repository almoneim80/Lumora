namespace Lumora.Infrastructure.Repositories
{
    public class ProgramCourseRepository(PgDbContext dbContext) : IProgramCourseRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task AddCourseAsync(ProgramCourse course, CancellationToken ct)
            => await _dbContext.ProgramCourses.AddAsync(course, ct);

        public async Task AddLessonsRangeAsync(IEnumerable<CourseLesson> lessons, CancellationToken ct)
            => await _dbContext.CourseLessons.AddRangeAsync(lessons, ct);

        public async Task AddAttachmentsRangeAsync(IEnumerable<LessonAttachment> attachments, CancellationToken ct)
            => await _dbContext.LessonAttachments.AddRangeAsync(attachments, ct);

        public async Task AddTestsRangeAsync(IEnumerable<Test> tests, CancellationToken ct)
            => await _dbContext.Tests.AddRangeAsync(tests, ct);

        public async Task AddQuestionsRangeAsync(IEnumerable<TestQuestion> questions, CancellationToken ct)
            => await _dbContext.TestQuestions.AddRangeAsync(questions, ct);

        public async Task AddChoicesRangeAsync(IEnumerable<TestChoice> choices, CancellationToken ct)
            => await _dbContext.TestChoices.AddRangeAsync(choices, ct);

        public async Task<ProgramCourse?> GetByIdAsync(int id, CancellationToken ct, bool asNoTracking = false)
        {
            var query = _dbContext.ProgramCourses.AsQueryable();
            if (asNoTracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(pc => pc.Id == id && !pc.IsDeleted, ct);
        }

        public async Task<TrainingProgram?> GetProgramByIdAsync(int id, CancellationToken ct)
            => await _dbContext.TrainingPrograms.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
    }
}
