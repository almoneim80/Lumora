namespace Lumora.Application.Interfaces.ProgramIntf
{
    public interface ITrainingProgramRepository
    {
        Task<TrainingProgram?> GetByIdAsync(int id, CancellationToken ct);
        Task<TrainingProgram?> GetFullDetailsByIdAsync(int id, CancellationToken ct);
        Task<List<TrainingProgram>> GetAllWithDetailsAsync(CancellationToken ct);
        Task<List<ProgramCourse>> GetCoursesWithDetailsAsync(int programId, CancellationToken ct);
        Task<ProgramCompletionData?> GetCompletionDataAsync(int programId, string userId, CancellationToken ct);
        Task<bool> HasCoursesAsync(int programId, CancellationToken ct);
        void Add(TrainingProgram entity);
    }
}
