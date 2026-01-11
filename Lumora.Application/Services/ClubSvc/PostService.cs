namespace Lumora.Application.Services.Club
{
    public class PostService(
            IPostRepository postRepository,
            ILogger<PostService> logger,
            ClubMessage messages,
            IUnitOfWork unitOfWork) : IPostService
    {
        private readonly IPostRepository _postRepository = postRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<PostService> _logger = logger;
        private readonly ClubMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> CreateAsync(PostCreateDto dto, string userId, CancellationToken cancellationToken, string createdByIp, string createdByAgent)
        {
            try
            {
                if (dto == null) return new GeneralResult(false, _messages.MsgPostDtoNull, null, ErrorType.BadRequest);
                if (string.IsNullOrWhiteSpace(dto.Content)) return new GeneralResult(false, _messages.MsgPostContentRequired, null, ErrorType.BadRequest);

                var post = new ClubPost
                {
                    UserId = userId,
                    Content = dto.Content.Trim(),
                    MediaUrl = dto.MediaFile,
                    MediaType = dto.MediaType ?? MediaType.Other,
                    Status = ClubPostStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ById = userId,
                    ByIp = createdByIp,
                    ByAgent = createdByAgent
                };

                await _postRepository.AddAsync(post, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgPostCreated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - CreateAsync Error");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Create Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteAsync(int postId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(postId, cancellationToken, tracked: true);

                if (post == null) return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                if (post.ById != userId) return new GeneralResult(false, _messages.MsgUnauthorizedDelete, null, ErrorType.Forbidden);

                post.IsDeleted = true;
                post.DeletedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new GeneralResult(true, _messages.MsgPostDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - DeleteAsync Error");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Delete Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PostDetailsDto>> GetByIdAsync(int postId, CancellationToken cancellationToken)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(postId, cancellationToken, includeUser: true);

                if (post == null || post.Status != ClubPostStatus.Approved)
                    return new GeneralResult<PostDetailsDto>(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);

                return new GeneralResult<PostDetailsDto>(true, _messages.MsgPostRetrieved, MapToDto(post), ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - GetByIdAsync Error");
                return new GeneralResult<PostDetailsDto>(false, _messages.GetUnexpectedErrorMessage("Get Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<PostDetailsDto>>> GetAllPublicAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var (items, totalCount) = await _postRepository.GetPagedPostsAsync(pagination, null, null, true, cancellationToken);
                return BuildPagedResult(items, totalCount, pagination, _messages.MsgPostsRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - GetAllPublicAsync Error");
                return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get Posts"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<PostDetailsDto>>> GetAllByUserAsync(string userId, PaginationRequestDto pagination, bool isAdmin, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);

                var (items, totalCount) = await _postRepository.GetPagedPostsAsync(pagination, null, userId, !isAdmin, cancellationToken);
                return BuildPagedResult(items, totalCount, pagination, _messages.MsgUserPostsRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - GetAllByUserAsync Error");
                return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get User Posts"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ReviewPostAsync(PostStatusUpdateDto dto, string adminId, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null || dto.PostId <= 0) return new GeneralResult(false, _messages.MsgPostIdInvalid, null, ErrorType.BadRequest);

                var post = await _postRepository.GetByIdAsync(dto.PostId, cancellationToken, tracked: true);
                if (post == null) return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                if (post.Status != ClubPostStatus.Pending) return new GeneralResult(false, _messages.MsgPostAlreadyReviewed, null, ErrorType.BadRequest);

                if (dto.NewStatus == ClubPostStatus.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
                    return new GeneralResult(false, _messages.MsgRejectionReasonRequired, null, ErrorType.BadRequest);

                post.Status = dto.NewStatus;
                post.ApprovedAt = dto.NewStatus == ClubPostStatus.Approved ? DateTimeOffset.UtcNow : null;
                post.UpdatedAt = DateTimeOffset.UtcNow;
                post.ById = adminId;
                post.Note = dto.RejectionReason?.Trim() ?? "";

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new GeneralResult(true, _messages.MsgPostReviewed, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - ReviewPostAsync Error");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Review Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<PostDetailsDto>>> GetPendingPostsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var (items, totalCount) = await _postRepository.GetPagedPostsAsync(pagination, ClubPostStatus.Pending, null, false, cancellationToken);
                return BuildPagedResult(items, totalCount, pagination, _messages.MsgPendingPostsRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - GetPendingPostsAsync Error");
                return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get Pending Posts"), null, ErrorType.InternalServerError);
            }
        }

        private PostDetailsDto MapToDto(ClubPost p) => new PostDetailsDto
        {
            Id = p.Id,
            Content = p.Content,
            MediaUrl = p.MediaUrl ?? "",
            MediaType = p.MediaType,
            Status = p.Status,
            CreatedAt = p.CreatedAt ?? DateTimeOffset.UtcNow,
            ApprovedAt = p.ApprovedAt,
            CreatorInfo = p.User == null ? null : new PostCreatorData
            {
                FullName = p.User.FullName,
                PhoneNumber = p.User.PhoneNumber,
                City = p.User.City,
                Sex = p.User.Sex,
                Avatar = p.User.Avatar,
            }
        };

        private GeneralResult<PagedResult<PostDetailsDto>> BuildPagedResult(IEnumerable<ClubPost> items, int total, PaginationRequestDto p, string msg)
        {
            if (!items.Any()) return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.MsgNoPostsFound, null, ErrorType.NotFound);

            return new GeneralResult<PagedResult<PostDetailsDto>>(true, msg, new PagedResult<PostDetailsDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize
            }, ErrorType.Success);
        }
    }
}
