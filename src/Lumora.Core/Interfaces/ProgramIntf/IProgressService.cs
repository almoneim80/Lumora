namespace Lumora.Interfaces.ProgramIntf
{
    public interface IProgressService
    {
        // Level: Lesson

        /// <summary>
        /// Marks a lesson as completed for a specific user.
        /// </summary>
        Task<GeneralResult> MarkLessonCompletedAsync(int lessonId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the progress details for a specific lesson and user.
        /// </summary>
        Task<GeneralResult<LessonProgressDetailsDto>> GetLessonProgressAsync(string userId, int lessonId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the completed lessons for a specific user and course.
        /// </summary>
        Task<GeneralResult<List<LessonProgressDetailsDto>>> GetCompletedLessonsAsync(string userId, int courseId, CancellationToken cancellationToken);

        // Level: Course

        /// <summary>
        /// Updates the progress of a course for a specific user.
        /// </summary>
        Task<GeneralResult> UpdateProgramCourseProgressAsync(int courseId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the progress details for a specific course and user.
        /// </summary>
        Task<GeneralResult<CourseProgressDetailsDto>> GetProgramCourseProgressAsync(string userId, int courseId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the progress details for all courses for a specific user.
        /// </summary>
        Task<GeneralResult<List<CourseProgressDetailsDto>>> GetUserCoursesProgressAsync(string userId, CancellationToken cancellationToken);

        // Level: Program

        /// <summary>
        /// Updates the progress of a program for a specific user.
        /// </summary>
        Task<GeneralResult> UpdateProgramProgressAsync(int programId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the progress details for a specific program and user.
        /// </summary>
        Task<GeneralResult<ProgramProgressDetailsDto>> GetProgramProgressAsync(string userId, int programId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the progress details for all programs for a specific user.
        /// </summary>
        Task<GeneralResult<List<ProgramProgressDetailsDto>>> GetUserProgramsProgressAsync(string userId, CancellationToken cancellationToken);

        // Time Tracking

        /// <summary>
        /// Starts a lesson session for a specific user.
        /// </summary>
        Task<GeneralResult> StartLessonSessionAsync(string userId, int lessonId, CancellationToken cancellationToken);

        /// <summary>
        /// Ends a lesson session for a specific user.
        /// </summary>
        Task<GeneralResult> EndLessonSessionAsync(string userId, int lessonId, CancellationToken cancellationToken);


        // Internal Utility

        /// <summary>
        /// Syncs the progress of all users for a specific program.
        /// </summary>
        Task<GeneralResult> SyncAllUserProgressForProgramAsync(int programId, CancellationToken cancellationToken);
    }
}
