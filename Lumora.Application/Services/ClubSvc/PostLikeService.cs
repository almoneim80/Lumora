namespace Lumora.Application.Services.Club
{
    public class PostLikeService(
        IPostLikeRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<PostLikeService> logger,
        ClubMessage messages) : IPostLikeService
    {
        private readonly IPostLikeRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<PostLikeService> _logger = logger;
        private readonly ClubMessage _messages = messages;

        public async Task<GeneralResult> LikeAsync(int postId, string userId, CancellationToken cancellationToken, string createdByIp, string createdByAgent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("PostLikeService - LikeAsync: userId is null or empty.");
                    return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var post = await _repository.GetApprovedPostByIdAsync(postId, cancellationToken);
                if (post == null)
                {
                    _logger.LogWarning("PostLikeService - LikeAsync: Post {PostId} not found/approved.", postId);
                    return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                }

                var existingLike = await _repository.GetLikeAsync(postId, userId, cancellationToken);
                if (existingLike != null)
                {
                    return new GeneralResult(true, _messages.MsgPostAlreadyLiked, null, ErrorType.Success);
                }

                var like = new ClubPostLike
                {
                    ClubPostId = postId,
                    UserId = userId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ById = userId,
                    ByIp = createdByIp,
                    ByAgent = createdByAgent
                };

                _repository.AddLike(like);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgPostLikedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostLikeService - LikeAsync: Error for post {PostId}.", postId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Like Post"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> UnlikeAsync(int postId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var postExists = await _repository.IsPostApprovedAsync(postId, cancellationToken);
                if (!postExists)
                {
                    return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                }

                var existingLike = await _repository.GetLikeAsync(postId, userId, cancellationToken);
                if (existingLike == null)
                {
                    return new GeneralResult(false, _messages.MsgPostNotYetLiked, null, ErrorType.BadRequest);
                }

                _repository.RemoveLike(existingLike);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgPostUnlikedSuccessfully, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostLikeService - UnlikeAsync: Error for post {PostId}.", postId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Unlike Post"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<bool>> HasLikedAsync(int postId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return new GeneralResult<bool>(false, _messages.MsgUserIdRequired, false, ErrorType.BadRequest);

                var postExists = await _repository.IsPostApprovedAsync(postId, cancellationToken);
                if (!postExists)
                    return new GeneralResult<bool>(false, _messages.MsgPostNotFound, false, ErrorType.NotFound);

                var hasLiked = await _repository.HasUserLikedPostAsync(postId, userId, cancellationToken);
                return new GeneralResult<bool>(true, _messages.MsgLikeStatusRetrieved, hasLiked, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostLikeService - HasLikedAsync: Error for post {PostId}.", postId);
                return new GeneralResult<bool>(false, _messages.GetUnexpectedErrorMessage("Check Like Status"), false, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<int>> GetLikeCountAsync(int postId, CancellationToken cancellationToken)
        {
            try
            {
                var postExists = await _repository.IsPostApprovedAsync(postId, cancellationToken);
                if (!postExists)
                    return new GeneralResult<int>(false, _messages.MsgPostNotFound, 0, ErrorType.NotFound);

                var likeCount = await _repository.GetLikeCountAsync(postId, cancellationToken);
                return new GeneralResult<int>(true, _messages.MsgPostLikeCountRetrieved, likeCount, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostLikeService - GetLikeCountAsync: Error for post {PostId}.", postId);
                return new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("Get Like Count"), 0, ErrorType.InternalServerError);
            }
        }
    }
}
