namespace Lumora.Interfaces.ProgramIntf
{
    public interface ITrainingProgramService
    {
        /// <summary>
        /// Create a new  training program.
        /// </summary>
        Task<GeneralResult> CreateProgramAsync(TrainingProgramCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Get all training programs.
        /// </summary>
        Task<GeneralResult<List<TrainingProgramFullDetailsDto>>> GetAllProgramsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Get one training program.
        /// </summary>
        Task<GeneralResult<TrainingProgramFullDetailsDto>> GetOneProgramAsync(int programId, CancellationToken cancellationToken);

        /// <summary>
        /// Update one training program.
        /// </summary>
        Task<GeneralResult> UpdateProgramAsync(int programId, TrainingProgramUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Delete one training program.
        /// </summary>
        Task<GeneralResult> DeleteProgramAsync(int programId, CancellationToken cancellationToken);

        /// <summary>
        /// Get the completion rate of a program.
        /// </summary>
        Task<GeneralResult<ProgramCompletionData>> ProgramCompletionRateAsync(int programId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Get all courses in a program.
        /// </summary>
        Task<GeneralResult<List<CourseFullDetailsDto>>> GetProgramCoursesAsync(int programId, CancellationToken cancellationToken);
    }
}
