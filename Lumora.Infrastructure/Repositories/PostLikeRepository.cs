using Lumora.Application.Interfaces.ClubIntf;

namespace Lumora.Infrastructure.Repositories
{
    public class PostLikeRepository(PgDbContext dbContext) : IPostLikeRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<ClubPost?> GetApprovedPostByIdAsync(int postId, CancellationToken ct)
        {
            return await _dbContext.ClubPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, ct);
        }

        public async Task<ClubPostLike?> GetLikeAsync(int postId, string userId, CancellationToken ct)
        {
            return await _dbContext.ClubPostLikes
                .FirstOrDefaultAsync(l => l.ClubPostId == postId && l.UserId == userId, ct);
        }

        public async Task<bool> IsPostApprovedAsync(int postId, CancellationToken ct)
        {
            return await _dbContext.ClubPosts
                .AsNoTracking()
                .AnyAsync(p => p.Id == postId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, ct);
        }

        public async Task<bool> HasUserLikedPostAsync(int postId, string userId, CancellationToken ct)
        {
            return await _dbContext.ClubPostLikes
                .AsNoTracking()
                .AnyAsync(l => l.ClubPostId == postId && l.UserId == userId, ct);
        }

        public async Task<int> GetLikeCountAsync(int postId, CancellationToken ct)
        {
            return await _dbContext.ClubPostLikes
                .AsNoTracking()
                .CountAsync(l => l.ClubPostId == postId, ct);
        }

        public void AddLike(ClubPostLike like)
        {
            _dbContext.ClubPostLikes.Add(like);
        }

        public void RemoveLike(ClubPostLike like)
        {
            _dbContext.ClubPostLikes.Remove(like);
        }
    }
}
