namespace Lumora.Application.Interfaces.TestIntf
{
    public interface ITestAttemptRepository
    {
        Task<TestAttempt?> GetByIdWithAnswersAsync(int attemptId, CancellationToken ct);
        Task<int> GetUserAttemptsCountAsync(string userId, int testId, CancellationToken ct);
        Task<decimal?> GetUserBestScoreAsync(string userId, int testId, int excludeAttemptId, CancellationToken ct);
        Task<TestAttempt?> GetBestAttemptWithDetailsAsync(string userId, int testId, CancellationToken ct);
        IQueryable<TestAttempt> GetUserAttemptsQueryable(string userId, int testId);

        Task AddAttemptAsync(TestAttempt attempt, CancellationToken ct);
        Task AddAnswerAsync(TestAnswer answer, CancellationToken ct);
        void RemoveAnswer(TestAnswer answer);

        // عمليات مساعدة لجلب بيانات مرتبطة بالـ Attempt
        Task<decimal> GetTotalTestMarkAsync(int testId, CancellationToken ct);
        Task<List<TestAnswer>> GetCorrectAnswersWithMarksAsync(int attemptId, CancellationToken ct);
        Task<PagedResult<TestAttempt>> GetUserAttemptsPagedAsync(string userId, int testId, int skip, int pageSize, CancellationToken ct);
    }
}
