using Lumora.Application.Interfaces.ClubIntf;

namespace Lumora.Infrastructure.Repositories
{
    public class AmbassadorRepository(PgDbContext dbContext) : IAmbassadorRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<ClubAmbassador?> GetByIdAsync(int id, CancellationToken ct)
            => await _dbContext.ClubAmbassadors.FirstOrDefaultAsync(a => a.Id == id, ct);

        public async Task<User?> GetUserForAmbassadorAsync(string userId, CancellationToken ct)
            => await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);

        public async Task<bool> HasOverlappingAmbassadorAsync(DateTimeOffset now, CancellationToken ct)
            => await _dbContext.ClubAmbassadors.AnyAsync(a => now >= a.StartDate && now <= a.EndDate, ct);

        public async Task<ClubPost?> GetApprovedPostAsync(int postId, CancellationToken ct)
            => await _dbContext.ClubPosts.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, ct);

        public async Task AddAsync(ClubAmbassador ambassador, CancellationToken ct)
            => await _dbContext.ClubAmbassadors.AddAsync(ambassador, ct);

        public void Update(ClubAmbassador ambassador)
            => _dbContext.ClubAmbassadors.Update(ambassador);

        public async Task<ClubAmbassador?> GetActiveAmbassadorAsync(DateTimeOffset now, CancellationToken ct)
            => await _dbContext.ClubAmbassadors
                .Include(a => a.ClubPost)
                .Include(a => a.User)
                .Where(a => a.StartDate <= now && a.EndDate >= now)
                .OrderByDescending(a => a.StartDate)
                .FirstOrDefaultAsync(ct);

        public async Task<List<ClubAmbassador>> GetHistoryAsync(DateTimeOffset now, CancellationToken ct)
            => await _dbContext.ClubAmbassadors
                .Include(a => a.ClubPost)
                .Include(a => a.User)
                .Where(a => a.EndDate != null && a.EndDate <= now)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync(ct);
    }
}
