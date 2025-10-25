namespace Lumora.Interfaces.ProgramIntf
{
    public interface ICourseLessonService
    {
        /// <summary>
        /// Create lesson with content.
        /// </summary>
        Task<GeneralResult> CreateLessonWithContentAsync(int courseId, LessonsWithContentCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Update lesson data.
        /// </summary>
        Task<GeneralResult> UpdateLessonAsync(int lessonId, LessonUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Get lessons with content by course id.
        /// </summary>
        Task<GeneralResult<List<LessonFullDetailsDto>>> GetLessonsWithContentByCourseIdAsync(int courseId, CancellationToken cancellationToken);

        /// <summary>
        /// Soft delete lesson.
        /// </summary>
        Task<GeneralResult> SoftDeleteLessonAsync(int lessonId, CancellationToken cancellationToken);
    }
}
