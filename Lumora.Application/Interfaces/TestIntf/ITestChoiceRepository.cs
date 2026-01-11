namespace Lumora.Application.Interfaces.TestIntf
{
    public interface ITestChoiceRepository
    {
        Task<TestChoice?> GetByIdAsync(int id, CancellationToken ct);
        Task<bool> QuestionExistsAsync(int questionId, CancellationToken ct);
        Task<bool> HasCorrectChoiceAsync(int questionId, int? excludeChoiceId, CancellationToken ct);
        Task<int> GetChoicesCountAsync(int questionId, CancellationToken ct);
        Task AddAsync(TestChoice choice, CancellationToken ct);
        Task<List<TestChoice>> GetChoicesByQuestionIdAsync(int questionId, CancellationToken ct);
        void Update(TestChoice choice);
        void UpdateRange(IEnumerable<TestChoice> choices);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
