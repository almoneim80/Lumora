namespace Lumora.Application.Interfaces.ClubIntf
{
    public interface IPostRepository
    {
        Task<ClubPost?> GetByIdAsync(int id, CancellationToken ct, bool includeUser = false, bool tracked = false);
        Task AddAsync(ClubPost post, CancellationToken ct);
        Task UpdateAsync(ClubPost post, CancellationToken ct);
        Task<(IEnumerable<ClubPost> Items, int TotalCount)> GetPagedPostsAsync(
            PaginationRequestDto pagination,
            ClubPostStatus? status,
            string? userId,
            bool onlyApproved,
            CancellationToken ct);
    }
}
