namespace Lumora.Application.Interfaces.ClubIntf
{
    public interface IPostLikeRepository
    {
        Task<ClubPost?> GetApprovedPostByIdAsync(int postId, CancellationToken ct);
        Task<ClubPostLike?> GetLikeAsync(int postId, string userId, CancellationToken ct);
        Task<bool> IsPostApprovedAsync(int postId, CancellationToken ct);
        Task<bool> HasUserLikedPostAsync(int postId, string userId, CancellationToken ct);
        Task<int> GetLikeCountAsync(int postId, CancellationToken ct);
        void AddLike(ClubPostLike like);
        void RemoveLike(ClubPostLike like);
    }
}
