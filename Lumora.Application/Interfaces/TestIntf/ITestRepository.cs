namespace Lumora.Application.Interfaces.TestIntf
{
    public interface ITestRepository
    {
        Task<Test?> GetTestByIdAsync(int testId, bool includeRelated, CancellationToken ct);
        Task<Test?> GetActiveTestForLessonAsync(int lessonId, CancellationToken ct);
        Task<CourseLesson?> GetLessonAsync(int lessonId, CancellationToken ct);
        IQueryable<Test> GetTestsQueryable();
        Task<PagedResult<Test>> GetPagedTestsAsync(PaginationRequestDto pagination, CancellationToken ct);

        Task AddTestAsync(Test test, CancellationToken ct);
        Task AddTestQuestionAsync(TestQuestion question, CancellationToken ct);
        Task AddTestChoiceAsync(TestChoice choice, CancellationToken ct);
    }
}
