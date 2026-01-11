namespace Lumora.Infrastructure.Repositories
{
    public class TestChoiceRepository(PgDbContext dbContext) : ITestChoiceRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<TestChoice?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _dbContext.TestChoices
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public async Task<bool> QuestionExistsAsync(int questionId, CancellationToken ct)
        {
            return await _dbContext.TestQuestions
                .AnyAsync(q => q.Id == questionId && !q.IsDeleted, ct);
        }

        public async Task<bool> HasCorrectChoiceAsync(int questionId, int? excludeChoiceId, CancellationToken ct)
        {
            var query = _dbContext.TestChoices
                .Where(c => c.TestQuestionId == questionId && !c.IsDeleted && c.IsCorrect);

            if (excludeChoiceId.HasValue)
                query = query.Where(c => c.Id != excludeChoiceId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task<int> GetChoicesCountAsync(int questionId, CancellationToken ct)
        {
            return await _dbContext.TestChoices
                .CountAsync(c => c.TestQuestionId == questionId && !c.IsDeleted, ct);
        }

        public async Task AddAsync(TestChoice choice, CancellationToken ct)
        {
            await _dbContext.TestChoices.AddAsync(choice, ct);
        }

        public async Task<List<TestChoice>> GetChoicesByQuestionIdAsync(int questionId, CancellationToken ct)
        {
            return await _dbContext.TestChoices
                .Where(c => c.TestQuestionId == questionId && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync(ct);
        }

        public void Update(TestChoice choice)
        {
            _dbContext.TestChoices.Update(choice);
        }

        public void UpdateRange(IEnumerable<TestChoice> choices)
        {
            _dbContext.TestChoices.UpdateRange(choices);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
