using Lumora.Application.Interfaces.ClubIntf;

namespace Lumora.Infrastructure.Repositories
{
    public class PostRepository(PgDbContext dbContext) : IPostRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<ClubPost?> GetByIdAsync(int id, CancellationToken ct, bool includeUser = false, bool tracked = false)
        {
            IQueryable<ClubPost> query = _dbContext.ClubPosts;

            if (!tracked) query = query.AsNoTracking();
            if (includeUser) query = query.Include(p => p.User);

            return await query.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        }

        public async Task AddAsync(ClubPost post, CancellationToken ct)
        {
            await _dbContext.ClubPosts.AddAsync(post, ct);
        }

        public async Task UpdateAsync(ClubPost post, CancellationToken ct)
        {
            _dbContext.ClubPosts.Update(post);
            await Task.CompletedTask;
        }

        public async Task<(IEnumerable<ClubPost> Items, int TotalCount)> GetPagedPostsAsync(
            PaginationRequestDto pagination,
            ClubPostStatus? status,
            string? userId,
            bool onlyApproved,
            CancellationToken ct)
        {
            var query = _dbContext.ClubPosts
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => !p.IsDeleted);

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (onlyApproved)
                query = query.Where(p => p.Status == ClubPostStatus.Approved);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(p => p.UserId == userId);

            query = query.OrderByDescending(p => p.CreatedAt);

            var pagedResult = await query.ApplyPaginationAsync(pagination, ct);
            return (pagedResult.Items, pagedResult.TotalCount);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
