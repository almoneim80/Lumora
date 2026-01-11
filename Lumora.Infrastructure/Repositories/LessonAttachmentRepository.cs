namespace Lumora.Infrastructure.Repositories
{
    public class LessonAttachmentRepository : ILessonAttachmentRepository
    {
        private readonly PgDbContext _dbContext;

        public LessonAttachmentRepository(PgDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LessonAttachment?> GetByIdAsync(int id, bool includeLesson = false, CancellationToken ct = default)
        {
            var query = _dbContext.LessonAttachments.Where(l => l.Id == id && l.IsDeleted == false);

            if (includeLesson)
                query = query.Include(a => a.CourseLesson);

            return await query.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        }

        public async Task<List<LessonAttachment>> GetByLessonIdAsync(int lessonId, CancellationToken ct = default)
        {
            return await _dbContext.LessonAttachments
                .AsNoTracking()
                .Where(a => a.LessonId == lessonId && !a.IsDeleted)
                .ToListAsync(ct);
        }

        public async Task<bool> LessonExistsAsync(int lessonId, CancellationToken ct = default)
        {
            return await _dbContext.CourseLessons
                .AnyAsync(l => l.Id == lessonId && !l.IsDeleted, ct);
        }

        public async Task AddAsync(LessonAttachment attachment, CancellationToken ct = default)
        {
            await _dbContext.LessonAttachments.AddAsync(attachment, ct);
        }
    }
}
