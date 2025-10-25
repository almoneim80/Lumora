namespace Lumora.Interfaces.ProgramIntf
{
    public interface IProgramCourseService
    {
        /// <summary>
        /// Create course with content.
        /// </summary>
        Task<GeneralResult> CreateCourseWithContentAsync(CourseWithLessonsCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Update course data.
        /// </summary>
        Task<GeneralResult> UpdateCourseAsync(int courseId, CourseUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Get course with content by course id.
        /// </summary>
        Task<GeneralResult<CourseFullDetailsDto>> GetCourseWithContentByIdAsync(int courseId, CancellationToken cancellationToken);
    }
}
