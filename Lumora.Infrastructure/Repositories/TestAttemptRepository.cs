namespace Lumora.Infrastructure.Repositories
{
    public class TestAttemptRepository(PgDbContext dbContext) : ITestAttemptRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<TestAttempt?> GetByIdWithAnswersAsync(int attemptId, CancellationToken ct)
        {
            return await _dbContext.TestAttempts
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId && !a.IsDeleted, ct);
        }

        public async Task<int> GetUserAttemptsCountAsync(string userId, int testId, CancellationToken ct)
        {
            return await _dbContext.TestAttempts
                .CountAsync(a => a.TestId == testId && a.UserId == userId && !a.IsDeleted, ct);
        }

        public async Task<decimal?> GetUserBestScoreAsync(string userId, int testId, int excludeAttemptId, CancellationToken ct)
        {
            return await _dbContext.TestAttempts
                .Where(a => a.UserId == userId && a.TestId == testId && a.SubmittedAt != null && !a.IsDeleted && a.Id != excludeAttemptId)
                .MaxAsync(a => (decimal?)a.TotalMark, ct);
        }

        public async Task<TestAttempt?> GetBestAttemptWithDetailsAsync(string userId, int testId, CancellationToken ct)
        {
            return await _dbContext.TestAttempts
                .AsNoTracking()
                .Include(a => a.Test)
                .Include(a => a.Answers).ThenInclude(ans => ans.TestQuestion)
                .Include(a => a.Answers).ThenInclude(ans => ans.TestChoice)
                .Where(a => a.UserId == userId && a.TestId == testId && !a.IsDeleted && a.SubmittedAt != null && a.IsValidSubmission)
                .OrderByDescending(a => a.TotalMark)
                .FirstOrDefaultAsync(ct);
        }

        public IQueryable<TestAttempt> GetUserAttemptsQueryable(string userId, int testId)
        {
            return _dbContext.TestAttempts
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.TestId == testId && !a.IsDeleted)
                .OrderByDescending(a => a.StartedAt);
        }

        public async Task AddAttemptAsync(TestAttempt attempt, CancellationToken ct)
        {
            await _dbContext.TestAttempts.AddAsync(attempt, ct);
        }

        public async Task AddAnswerAsync(TestAnswer answer, CancellationToken ct)
        {
            await _dbContext.TestAnswers.AddAsync(answer, ct);
        }

        public void RemoveAnswer(TestAnswer answer)
        {
            _dbContext.TestAnswers.Remove(answer);
        }

        public async Task<decimal> GetTotalTestMarkAsync(int testId, CancellationToken ct)
        {
            return await _dbContext.TestQuestions
                .Where(q => q.TestId == testId && !q.IsDeleted)
                .SumAsync(q => q.Mark, ct);
        }

        public async Task<List<TestAnswer>> GetCorrectAnswersWithMarksAsync(int attemptId, CancellationToken ct)
        {
            return await _dbContext.TestAnswers
                .Include(a => a.TestQuestion)
                .Where(a => a.TestAttemptId == attemptId && !a.IsDeleted && a.IsCorrect)
                .ToListAsync(ct);
        }

        public async Task<PagedResult<TestAttempt>> GetUserAttemptsPagedAsync(string userId, int testId, int skip, int pageSize, CancellationToken ct)
        {
            var query = _dbContext.TestAttempts
                .Where(a => a.UserId == userId && a.TestId == testId);

            var totalCount = await query.CountAsync(ct);
            var items = await query.Skip(skip).Take(pageSize).ToListAsync(ct);

            return new PagedResult<TestAttempt>
            {
                Items = items,
                TotalCount = totalCount
            };
        }
    }
}
