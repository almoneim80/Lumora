namespace Lumora.Application.Interfaces.TestIntf
{
    public interface ITestQuestionRepository
    {
        Task<TestQuestion?> GetByIdWithChoicesAndTestAsync(int id, CancellationToken ct);
        Task<TestQuestion?> GetByIdWithChoicesAsync(int id, CancellationToken ct);
        Task<bool> TestExistsAsync(int testId, CancellationToken ct);
        Task<int> GetQuestionsCountAsync(int testId, CancellationToken ct);
        Task AddQuestionAsync(TestQuestion question, CancellationToken ct);
        Task AddChoiceAsync(TestChoice choice, CancellationToken ct);
        Task<TestChoice?> GetChoiceByIdAsync(int choiceId, CancellationToken ct);
        Task<Test?> GetTestWithQuestionsAsync(int testId, CancellationToken ct);
        Task<(List<TestQuestion> Items, int TotalCount)> GetPagedQuestionsByTestIdAsync(int testId, int pageNumber, int pageSize, CancellationToken ct);
        Task<List<TestQuestion>> GetQuestionsByIdsAsync(int testId, List<int> ids, CancellationToken ct);
        void UpdateQuestion(TestQuestion question);
        void UpdateTest(Test test);
    }
}
