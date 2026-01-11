namespace Lumora.Infrastructure.Repositories
{
    public class TrainingProgramRepository(PgDbContext dbContext) : ITrainingProgramRepository
    {
        public async Task<TrainingProgram?> GetByIdAsync(int id, CancellationToken ct) =>
            await dbContext.TrainingPrograms.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        public async Task<TrainingProgram?> GetFullDetailsByIdAsync(int id, CancellationToken ct) =>
            await dbContext.TrainingPrograms.AsNoTracking()
                .Include(p => p.ProgramCourses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Lessons.Where(l => !l.IsDeleted))
                        .ThenInclude(l => l.LessonAttachments.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        public async Task<List<TrainingProgram>> GetAllWithDetailsAsync(CancellationToken ct) =>
            await dbContext.TrainingPrograms.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProgramCourses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Lessons.Where(l => !l.IsDeleted))
                        .ThenInclude(l => l.LessonAttachments.Where(a => !a.IsDeleted))
                .ToListAsync(ct);

        public async Task<List<ProgramCourse>> GetCoursesWithDetailsAsync(int programId, CancellationToken ct) =>
            await dbContext.ProgramCourses.Where(c => c.ProgramId == programId && !c.IsDeleted).AsNoTracking()
                .Include(c => c.Lessons!).ThenInclude(l => l.LessonAttachments)
                .Include(c => c.Lessons!).ThenInclude(l => l.LessonTest)
                .ToListAsync(ct);

        public async Task<ProgramCompletionData?> GetCompletionDataAsync(int programId, string userId, CancellationToken ct) =>
            await dbContext.TraineeProgresses.AsNoTracking()
                .Where(p => p.ProgramId == programId && p.UserId == userId && !p.IsDeleted)
                .Select(d => new ProgramCompletionData
                {
                    IsCompleted = d.IsCompleted,
                    CompletionPercentage = d.CompletionPercentage,
                    TotalTimeSpent = d.TotalTimeSpent
                }).FirstOrDefaultAsync(ct);

        public async Task<bool> HasCoursesAsync(int programId, CancellationToken ct) =>
            await dbContext.ProgramCourses.AnyAsync(c => c.ProgramId == programId && !c.IsDeleted, ct);

        public void Add(TrainingProgram entity) => dbContext.TrainingPrograms.Add(entity);
    }
}
