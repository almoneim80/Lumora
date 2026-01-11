using Lumora.Application.DTOs.TrainingProgram;

namespace Lumora.Infrastructure.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly PgDbContext _dbContext;

        public EnrollmentRepository(PgDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProgramEnrollment?> GetEnrollmentAsync(string userId, int programId, CancellationToken ct)
        {
            return await _dbContext.ProgramEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.ProgramId == programId && !e.IsDeleted, ct);
        }

        public async Task<bool> IsEnrolledAsync(string userId, int programId, CancellationToken ct)
        {
            return await _dbContext.ProgramEnrollments.AsNoTracking()
                .AnyAsync(e => e.ProgramId == programId && e.UserId == userId &&
                               !e.IsDeleted && e.EnrollmentStatus == EnrollmentStatus.Active, ct);
        }

        public async Task<List<EnrollmentWithUserData>> GetEnrolledUsersAsync(int programId, CancellationToken ct)
        {
            return await _dbContext.ProgramEnrollments.AsNoTracking()
                .Include(e => e.User)
                .Where(e => e.ProgramId == programId && !e.IsDeleted && e.EnrollmentStatus == EnrollmentStatus.Active)
                .Select(e => new EnrollmentWithUserData
                {
                    FullName = e.User.FullName,
                    Email = e.User.Email,
                    EnrolledAt = e.EnrolledAt,
                    EnrollmentStatus = e.EnrollmentStatus
                }).ToListAsync(ct);
        }

        public async Task<EnrollmentWithUserData?> GetUserEnrollmentInfoAsync(string userId, int programId, CancellationToken ct)
        {
            return await _dbContext.ProgramEnrollments.AsNoTracking()
                .Where(e => e.UserId == userId && e.ProgramId == programId && !e.IsDeleted && e.EnrollmentStatus == EnrollmentStatus.Active)
                .Select(e => new EnrollmentWithUserData
                {
                    FullName = e.User.FullName,
                    Email = e.User.Email,
                    EnrolledAt = e.EnrolledAt,
                    EnrollmentStatus = e.EnrollmentStatus
                }).FirstOrDefaultAsync(ct);
        }

        public void Add(ProgramEnrollment enrollment)
        {
            _dbContext.ProgramEnrollments.Add(enrollment);
        }

        public async Task<ProgramEnrollment?> GetActiveEnrollmentAsync(string userId, int programId, CancellationToken ct)
        {
            return await _dbContext.ProgramEnrollments
                .FirstOrDefaultAsync(e =>
                    e.UserId == userId &&
                    e.ProgramId == programId &&
                    e.EnrollmentStatus == EnrollmentStatus.Active &&
                    !e.IsDeleted, ct);
        }

        public void Update(ProgramEnrollment enrollment)
        {
            _dbContext.ProgramEnrollments.Update(enrollment);
        }
    }
}
