using Lumora.Interfaces.ClubIntf;
namespace Lumora.Services.Club
{
    public class PostLikeService(PgDbContext dbContext, ILogger<PostLikeService> logger, ClubMessage messages, IHttpContextHelper httpContextHelper) : IPostLikeService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<PostLikeService> _logger = logger;
        private readonly ClubMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> LikeAsync(int postId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("PostLikeService - LikeAsync: userId is null or empty.");
                    return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var post = await _dbContext.ClubPosts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, cancellationToken);

                if (post == null)
                {
                    _logger.LogWarning("PostLikeService - LikeAsync: Post with ID {PostId} not found or not approved.", postId);
                    return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                }

                var existingLike = await _dbContext.ClubPostLikes
                    .FirstOrDefaultAsync(l => l.ClubPostId == postId && l.UserId == userId, cancellationToken);

                if (existingLike != null)
                {
                    _logger.LogInformation("PostLikeService - LikeAsync: User {UserId} already liked post {PostId}.", userId, postId);
                    return new GeneralResult(true, _messages.MsgPostAlreadyLiked, null, ErrorType.Success);
                }

                var like = new ClubPostLike
                {
                    ClubPostId = postId,
                    UserId = userId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ById = userId,
                    ByIp = httpContextHelper.IpAddress,
                    ByAgent = httpContextHelper.UserAgent
                };

                await _dbContext.ClubPostLikes.AddAsync(like, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PostLikeService - LikeAsync: User {UserId} liked post {PostId}.", userId, postId);
                return new GeneralResult(true, _messages.MsgPostLikedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostLikeService - LikeAsync: Unexpected error while liking post {PostId}.", postId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Like Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UnlikeAsync(int postId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("PostLikeService - UnlikeAsync: userId is null or empty.");
                    return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var post = await _dbContext.ClubPosts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, cancellationToken);

                if (post == null)
                {
                    _logger.LogWarning("PostLikeService - UnlikeAsync: Post with ID {PostId} not found or not approved.", postId);
                    return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                }

                var existingLike = await _dbContext.ClubPostLikes
                    .FirstOrDefaultAsync(l => l.ClubPostId == postId && l.UserId == userId, cancellationToken);

                if (existingLike == null)
                {
                    _logger.LogInformation("PostLikeService - UnlikeAsync: User {UserId} has not liked post {PostId}.", userId, postId);
                    return new GeneralResult(false, _messages.MsgPostNotYetLiked, null, ErrorType.BadRequest);
                }

                _dbContext.ClubPostLikes.Remove(existingLike);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PostLikeService - UnlikeAsync: User {UserId} unliked post {PostId}.", userId, postId);
                return new GeneralResult(true, _messages.MsgPostUnlikedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostLikeService - UnlikeAsync: Unexpected error while unliking post {PostId}.", postId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Unlike Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<bool>> HasLikedAsync(int postId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("PostLikeService - HasLikedAsync: userId is null or empty.");
                    return new GeneralResult<bool>(false, _messages.MsgUserIdRequired, false, ErrorType.BadRequest);
                }

                var postExists = await _dbContext.ClubPosts
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == postId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, cancellationToken);

                if (!postExists)
                {
                    _logger.LogWarning("PostLikeService - HasLikedAsync: Post with ID {PostId} not found or not approved.", postId);
                    return new GeneralResult<bool>(false, _messages.MsgPostNotFound, false, ErrorType.NotFound);
                }

                var hasLiked = await _dbContext.ClubPostLikes
                    .AsNoTracking()
                    .AnyAsync(l => l.ClubPostId == postId && l.UserId == userId, cancellationToken);

                _logger.LogInformation("PostLikeService - HasLikedAsync: User {UserId} like status for post {PostId}: {HasLiked}.", userId, postId, hasLiked);

                return new GeneralResult<bool>(true, _messages.MsgLikeStatusRetrieved, hasLiked, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostLikeService - HasLikedAsync: Unexpected error checking like status for post {PostId}.", postId);
                return new GeneralResult<bool>(false, _messages.GetUnexpectedErrorMessage("Check Like Status"), false, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<int>> GetLikeCountAsync(int postId, CancellationToken cancellationToken)
        {
            try
            {
                var postExists = await _dbContext.ClubPosts
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == postId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, cancellationToken);

                if (!postExists)
                {
                    _logger.LogWarning("PostLikeService - GetLikeCountAsync: Post with ID {PostId} not found or not approved.", postId);
                    return new GeneralResult<int>(false, _messages.MsgPostNotFound, 0, ErrorType.NotFound);
                }

                var likeCount = await _dbContext.ClubPostLikes
                    .AsNoTracking()
                    .CountAsync(l => l.ClubPostId == postId, cancellationToken);

                _logger.LogInformation("PostLikeService - GetLikeCountAsync: Post {PostId} has {LikeCount} likes.", postId, likeCount);

                return new GeneralResult<int>(true, _messages.MsgPostLikeCountRetrieved, likeCount, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostLikeService - GetLikeCountAsync: Unexpected error while counting likes for post {PostId}.", postId);
                return new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("Get Like Count"), 0, ErrorType.InternalServerError);
            }
        }
    }
}
