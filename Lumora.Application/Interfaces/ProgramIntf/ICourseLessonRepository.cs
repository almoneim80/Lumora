namespace Lumora.Application.Interfaces.ProgramIntf
{
    public interface ICourseLessonRepository
    {
        Task<bool> CourseExistsAsync(int courseId, CancellationToken ct);
        Task<CourseLesson?> GetLessonWithFullContentAsync(int lessonId, CancellationToken ct);
        Task<List<CourseLesson>> GetLessonsWithContentByCourseIdAsync(int courseId, CancellationToken ct);
        Task<CourseLesson?> GetByIdAsync(int lessonId, CancellationToken ct);
        Task AddLessonAsync(CourseLesson lesson, CancellationToken ct);
        Task AddAttachmentsRangeAsync(IEnumerable<LessonAttachment> attachments, CancellationToken ct);
        Task AddTestAsync(Test test, CancellationToken ct);
    }
}
