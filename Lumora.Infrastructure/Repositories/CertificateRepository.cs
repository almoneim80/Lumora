namespace Lumora.Infrastructure.Repositories
{
    public class CertificateRepository(PgDbContext dbContext) : ICertificateRepository
    {
        public async Task<ProgramEnrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId, CancellationToken ct)
        {
            return await dbContext.ProgramEnrollments
                .Include(e => e.TrainingProgram)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted, ct);
        }

        public async Task<bool> IsProgramCompletedAsync(string userId, int programId, CancellationToken ct)
        {
            return await dbContext.TraineeProgresses.AsNoTracking()
                .AnyAsync(p => p.ProgramId == programId
                               && p.UserId == userId
                               && p.Level == ProgressLevel.Program
                               && p.IsCompleted
                               && !p.IsDeleted, ct);
        }

        public async Task<ProgramCertificate?> GetCertificateByEnrollmentIdAsync(int enrollmentId, CancellationToken ct)
        {
            return await dbContext.ProgramCertificates
                .FirstOrDefaultAsync(c => c.EnrollmentId == enrollmentId && !c.IsDeleted, ct);
        }

        public async Task<ProgramCertificate?> GetCertificateWithDetailsAsync(int certificateId, CancellationToken ct)
        {
            return await dbContext.ProgramCertificates
                .AsNoTracking()
                .Include(c => c.ProgramEnrollment).ThenInclude(e => e.TrainingProgram)
                .Include(c => c.ProgramEnrollment).ThenInclude(e => e.User)
                .FirstOrDefaultAsync(c => c.Id == certificateId && !c.IsDeleted, ct);
        }

        public async Task<int> GetIssuedCertificatesCountAsync(int programId, CancellationToken ct)
        {
            return await dbContext.ProgramCertificates.AsNoTracking()
                .CountAsync(c => c.ProgramEnrollment.ProgramId == programId
                                 && c.Status == CertificateStatus.Issued
                                 && !c.IsDeleted, ct);
        }

        public async Task<List<ProgramCertificate>> GetUserCertificatesAsync(string userId, CancellationToken ct)
        {
            return await dbContext.ProgramCertificates.AsNoTracking()
                .Include(c => c.ProgramEnrollment)
                    .ThenInclude(e => e.TrainingProgram)
                .Where(c => c.ProgramEnrollment.UserId == userId &&
                            c.Status == CertificateStatus.Issued &&
                            !c.IsDeleted)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync(ct);
        }

        public async Task<ProgramCertificate?> GetByVerificationCodeAsync(string code, CancellationToken ct)
        {
            return await dbContext.ProgramCertificates
                .Include(c => c.ProgramEnrollment).ThenInclude(e => e.TrainingProgram)
                .Include(c => c.ProgramEnrollment).ThenInclude(e => e.User)
                .FirstOrDefaultAsync(c => c.VerificationCode == code && !c.IsDeleted, ct);
        }

        public void Add(ProgramCertificate certificate) => dbContext.ProgramCertificates.Add(certificate);

        public async Task<bool> ProgramExistsAsync(int programId, CancellationToken ct)
        {
            return await dbContext.TrainingPrograms.AsNoTracking()
                .AnyAsync(p => p.Id == programId && !p.IsDeleted, ct);
        }

        public async Task<User?> GetUserForValidationAsync(string userId, CancellationToken ct)
        {
            return await dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, ct);
        }

        public async Task<ProgramCertificate?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await dbContext.ProgramCertificates
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public void Update(ProgramCertificate certificate)
        {
            dbContext.ProgramCertificates.Update(certificate);
        }
    }
}
