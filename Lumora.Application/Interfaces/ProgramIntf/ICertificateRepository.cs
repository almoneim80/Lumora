namespace Lumora.Application.Interfaces.ProgramIntf
{
    public interface ICertificateRepository
    {
        Task<ProgramEnrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId, CancellationToken ct);
        Task<bool> IsProgramCompletedAsync(string userId, int programId, CancellationToken ct);
        Task<ProgramCertificate?> GetCertificateByEnrollmentIdAsync(int enrollmentId, CancellationToken ct);
        Task<ProgramCertificate?> GetCertificateWithDetailsAsync(int certificateId, CancellationToken ct);
        Task<int> GetIssuedCertificatesCountAsync(int programId, CancellationToken ct);
        Task<List<ProgramCertificate>> GetUserCertificatesAsync(string userId, CancellationToken ct);
        Task<ProgramCertificate?> GetByVerificationCodeAsync(string code, CancellationToken ct);
        void Add(ProgramCertificate certificate);
        Task<bool> ProgramExistsAsync(int programId, CancellationToken ct);
        Task<User?> GetUserForValidationAsync(string userId, CancellationToken ct);
        Task<ProgramCertificate?> GetByIdAsync(int id, CancellationToken ct);
        void Update(ProgramCertificate certificate);
    }
}
