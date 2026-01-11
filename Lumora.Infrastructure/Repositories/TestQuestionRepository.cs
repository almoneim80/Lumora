namespace Lumora.Infrastructure.Repositories
{
    public class TestQuestionRepository(PgDbContext dbContext) : ITestQuestionRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<TestQuestion?> GetByIdWithChoicesAndTestAsync(int id, CancellationToken ct)
        {
            return await _dbContext.TestQuestions
                .Include(q => q.Choices)
                .Include(q => q.Test)
                    .ThenInclude(t => t.CourseLesson)
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, ct);
        }

        public async Task<TestQuestion?> GetByIdWithChoicesAsync(int id, CancellationToken ct)
        {
            return await _dbContext.TestQuestions
                .Include(q => q.Choices)
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted, ct);
        }

        public async Task<bool> TestExistsAsync(int testId, CancellationToken ct)
        {
            return await _dbContext.Tests.AnyAsync(t => t.Id == testId && !t.IsDeleted, ct);
        }

        public async Task<int> GetQuestionsCountAsync(int testId, CancellationToken ct)
        {
            return await _dbContext.TestQuestions.CountAsync(q => q.TestId == testId && !q.IsDeleted, ct);
        }

        public async Task AddQuestionAsync(TestQuestion question, CancellationToken ct)
        {
            await _dbContext.TestQuestions.AddAsync(question, ct);
        }

        public async Task AddChoiceAsync(TestChoice choice, CancellationToken ct)
        {
            await _dbContext.TestChoices.AddAsync(choice, ct);
        }

        public async Task<TestChoice?> GetChoiceByIdAsync(int choiceId, CancellationToken ct)
        {
            return await _dbContext.TestChoices.FirstOrDefaultAsync(c => c.Id == choiceId && !c.IsDeleted, ct);
        }

        public async Task<Test?> GetTestWithQuestionsAsync(int testId, CancellationToken ct)
        {
            return await _dbContext.Tests
                .Include(t => t.Questions)
                .Include(t => t.CourseLesson)
                .Include(t => t.Attempts)
                    .ThenInclude(a => a.Answers)
                .FirstOrDefaultAsync(t => t.Id == testId && !t.IsDeleted, ct);
        }

        public async Task<(List<TestQuestion> Items, int TotalCount)> GetPagedQuestionsByTestIdAsync(int testId, int pageNumber, int pageSize, CancellationToken ct)
        {
            var query = _dbContext.TestQuestions
                .AsNoTracking()
                .Where(q => q.TestId == testId && !q.IsDeleted);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Include(q => q.Choices)
                .OrderByDescending(q => q.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<List<TestQuestion>> GetQuestionsByIdsAsync(int testId, List<int> ids, CancellationToken ct)
        {
            return await _dbContext.TestQuestions
                .Where(q => q.TestId == testId && !q.IsDeleted && ids.Contains(q.Id))
                .ToListAsync(ct);
        }

        public void UpdateQuestion(TestQuestion question) => _dbContext.TestQuestions.Update(question);

        public void UpdateTest(Test test) => _dbContext.Tests.Update(test);
    }
}
