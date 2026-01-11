namespace Lumora.Infrastructure.Repositories
{
    public class TestRepository(PgDbContext dbContext) : ITestRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task AddTestAsync(Test test, CancellationToken ct)
        {
            await _dbContext.Tests.AddAsync(test, ct);
        }

        public async Task AddTestChoiceAsync(TestChoice choice, CancellationToken ct)
        {
            await _dbContext.TestChoices.AddAsync(choice, ct);
        }

        public async Task AddTestQuestionAsync(TestQuestion question, CancellationToken ct)
        {
            await _dbContext.TestQuestions.AddAsync(question, ct);
        }

        public async Task<Test?> GetActiveTestForLessonAsync(int lessonId, CancellationToken ct)
        {
            return await _dbContext.Tests
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.LessonId == lessonId && !t.IsDeleted, ct);
        }

        public IQueryable<Test> GetTestsQueryable()
        {
            return _dbContext.Tests
                .Include(t => t.CourseLesson)
                .Include(t => t.Questions).ThenInclude(q => q.Choices)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt);
        }

        public async Task<PagedResult<Test>> GetPagedTestsAsync(PaginationRequestDto pagination, CancellationToken ct)
        {
            // بناء الاستعلام داخلياً (Encapsulation)
            var query = _dbContext.Tests
                .AsNoTracking()
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .Include(t => t.CourseLesson)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Choices);

            // استخدام دالة Pagination
            return await query.ApplyPaginationAsync(pagination, ct);
        }

        public async Task<CourseLesson?> GetLessonAsync(int lessonId, CancellationToken ct)
        {
            return await _dbContext.CourseLessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, ct);
        }

        public async Task<Test?> GetTestByIdAsync(int testId, bool includeRelated, CancellationToken ct)
        {
            var query = _dbContext.Tests.AsQueryable();

            if (includeRelated)
            {
                query = query.Include(t => t.Questions).ThenInclude(q => q.Choices)
                             .Include(t => t.Attempts).ThenInclude(a => a.Answers)
                             .Include(t => t.CourseLesson);
            }

            return await query.FirstOrDefaultAsync(t => t.Id == testId && !t.IsDeleted, ct);
        }
    }
}
