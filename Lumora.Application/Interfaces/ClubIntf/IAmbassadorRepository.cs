namespace Lumora.Application.Interfaces.ClubIntf
{
    public interface IAmbassadorRepository
    {
        Task<ClubAmbassador?> GetByIdAsync(int id, CancellationToken ct);
        Task<User?> GetUserForAmbassadorAsync(string userId, CancellationToken ct);
        Task<bool> HasOverlappingAmbassadorAsync(DateTimeOffset now, CancellationToken ct);
        Task<ClubPost?> GetApprovedPostAsync(int postId, CancellationToken ct);
        Task AddAsync(ClubAmbassador ambassador, CancellationToken ct);
        void Update(ClubAmbassador ambassador);
        Task<ClubAmbassador?> GetActiveAmbassadorAsync(DateTimeOffset now, CancellationToken ct);
        Task<List<ClubAmbassador>> GetHistoryAsync(DateTimeOffset now, CancellationToken ct);
    }
}
