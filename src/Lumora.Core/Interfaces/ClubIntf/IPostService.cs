using Lumora.DTOs.Club;
namespace Lumora.Interfaces.Club
{
    public interface IPostService
    {
        /// <summary>
        /// Creates a new post (text, image, or video) and submits it for admin review.
        /// </summary>
        /// <param name="dto">Post creation data including content and optional media.</param>
        /// <param name="userId">The ID of the user creating the post.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result indicating success or failure.</returns>
        Task<GeneralResult> CreateAsync(PostCreateDto dto, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a post if it was created by the given user.
        /// </summary>
        /// <param name="postId">ID of the post to delete.</param>
        /// <param name="userId">ID of the user requesting deletion (must be the creator).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result indicating success or failure.</returns>
        Task<GeneralResult> DeleteAsync(int postId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves post details by ID, including content, media, status, and creator info.
        /// </summary>
        /// <param name="postId">The ID of the post to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result with post details if found.</returns>
        Task<GeneralResult<PostDetailsDto>> GetByIdAsync(int postId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a paginated list of all public posts (approved by admin).
        /// </summary>
        /// <param name="pagination">Pagination request data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result with paginated list of public posts.</returns>
        Task<GeneralResult<PagedResult<PostDetailsDto>>> GetAllPublicAsync(PaginationRequestDto pagination, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a paginated list of posts created by a specific user.
        /// </summary>
        /// <param name="userId">ID of the user whose posts to retrieve.</param>
        /// <param name="pagination">Pagination request data.</param>
        /// /// <param name="isAdmin">Indicates whether the request is made by an admin.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result with paginated list of user posts.</returns>
        Task<GeneralResult<PagedResult<PostDetailsDto>>> GetAllByUserAsync(string userId, PaginationRequestDto pagination, bool isAdmin, CancellationToken cancellationToken);

        /// <summary>
        /// Reviews a post by updating its status to approved or rejected.
        /// </summary>
        /// <param name="dto">Review data including new status and optional rejection reason.</param>
        /// <param name="adminId">ID of the admin performing the review.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result indicating success or failure of the review action.</returns>
        Task<GeneralResult> ReviewPostAsync(PostStatusUpdateDto dto, string adminId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a paginated list of posts that are pending admin review.
        /// </summary>
        /// <param name="pagination">Pagination request data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result with paginated list of pending posts.</returns>
        Task<GeneralResult<PagedResult<PostDetailsDto>>> GetPendingPostsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken);
    }
}
