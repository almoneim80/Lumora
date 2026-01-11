namespace Lumora.Application.Interfaces.ProgramIntf
{
    public interface IEnrollmentRepository
    {
        Task<ProgramEnrollment?> GetEnrollmentAsync(string userId, int programId, CancellationToken ct);
        Task<bool> IsEnrolledAsync(string userId, int programId, CancellationToken ct);
        Task<List<EnrollmentWithUserData>> GetEnrolledUsersAsync(int programId, CancellationToken ct);
        Task<EnrollmentWithUserData?> GetUserEnrollmentInfoAsync(string userId, int programId, CancellationToken ct);
        void Add(ProgramEnrollment enrollment);
        Task<ProgramEnrollment?> GetActiveEnrollmentAsync(string userId, int programId, CancellationToken ct);
        void Update(ProgramEnrollment enrollment);
    }
}
