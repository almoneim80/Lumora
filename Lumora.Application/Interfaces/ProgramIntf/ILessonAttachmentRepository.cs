namespace Lumora.Application.Interfaces.ProgramIntf
{
    public interface ILessonAttachmentRepository
    {
        Task<LessonAttachment?> GetByIdAsync(int id, bool includeLesson = false, CancellationToken ct = default);
        Task<List<LessonAttachment>> GetByLessonIdAsync(int lessonId, CancellationToken ct = default);
        Task<bool> LessonExistsAsync(int lessonId, CancellationToken ct = default);
        Task AddAsync(LessonAttachment attachment, CancellationToken ct = default);
    }
}
