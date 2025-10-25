namespace Lumora.Interfaces.ClubIntf
{
    public interface IPostLikeService
    {
        /// <summary>
        /// Adds a like to the specified post by the given user.
        /// </summary>
        /// <param name="postId">ID of the post to like.</param>
        /// <param name="userId">ID of the user liking the post.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result indicating success or failure.</returns>
        Task<GeneralResult> LikeAsync(int postId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Removes a previously added like from the specified post by the given user.
        /// </summary>
        /// <param name="postId">ID of the post to unlike.</param>
        /// <param name="userId">ID of the user unliking the post.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result indicating success or failure.</returns>
        Task<GeneralResult> UnlikeAsync(int postId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Checks whether the specified user has liked the given post.
        /// </summary>
        /// <param name="postId">ID of the post to check.</param>
        /// <param name="userId">ID of the user to verify.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result with a boolean indicating whether the user has liked the post.</returns>
        Task<GeneralResult<bool>> HasLikedAsync(int postId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the total number of likes for the specified post.
        /// </summary>
        /// <param name="postId">ID of the post to count likes for.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>General result with the total number of likes.</returns>
        Task<GeneralResult<int>> GetLikeCountAsync(int postId, CancellationToken cancellationToken);
    }
}
