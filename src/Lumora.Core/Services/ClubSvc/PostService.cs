using Lumora.DTOs.Club;
using Lumora.Extensions;
using Lumora.Interfaces.Club;
namespace Lumora.Services.Club
{
    public class PostService(PgDbContext dbContext, ILogger<PostService> logger, ClubMessage messages, IHttpContextHelper httpContextHelper) : IPostService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<PostService> _logger = logger;
        private readonly ClubMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> CreateAsync(PostCreateDto dto, string userId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("PostService - CreateAsync: PostCreateDto is null.");
                    return new GeneralResult(false, _messages.MsgPostDtoNull, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(dto.Content))
                {
                    _logger.LogWarning("PostService - CreateAsync: Content is required.");
                    return new GeneralResult(false, _messages.MsgPostContentRequired, null, ErrorType.BadRequest);
                }

                var post = new ClubPost
                {
                    UserId = userId,
                    Content = dto.Content.Trim(),
                    MediaUrl = dto.MediaFile,
                    MediaType = dto.MediaType ?? MediaType.Other,
                    Status = ClubPostStatus.Pending,
                    CreatedAt = now,
                    ById = userId,
                    ByIp = httpContextHelper.IpAddress,
                    ByAgent = httpContextHelper.UserAgent
                };

                await _dbContext.ClubPosts.AddAsync(post, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PostService - CreateAsync: Post created successfully with ID {PostId}", post.Id);
                return new GeneralResult(true, _messages.MsgPostCreated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - CreateAsync: Error while creating post.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Create Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteAsync(int postId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                var post = await _dbContext.ClubPosts
                    .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted, cancellationToken);

                if (post == null)
                {
                    _logger.LogWarning("PostService - DeleteAsync: Post with ID {PostId} not found.", postId);
                    return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                }

                if (post.ById != userId)
                {
                    _logger.LogWarning("PostService - DeleteAsync: User {UserId} not authorized to delete post {PostId}.", userId, postId);
                    return new GeneralResult(false, _messages.MsgUnauthorizedDelete, null, ErrorType.Forbidden);
                }

                post.IsDeleted = true;
                post.DeletedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PostService - DeleteAsync: Post {PostId} deleted by user {UserId}.", postId, userId);
                return new GeneralResult(true, _messages.MsgPostDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - DeleteAsync: Error while deleting post {PostId}.", postId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Delete Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PostDetailsDto>> GetByIdAsync(int postId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var post = await _dbContext.ClubPosts
                    .AsNoTracking()
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, cancellationToken);

                if (post == null)
                {
                    _logger.LogWarning("PostService - GetByIdAsync: Post with ID {PostId} not found.", postId);
                    return new GeneralResult<PostDetailsDto>(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                }

                var dto = new PostDetailsDto
                {
                    Id = post.Id,
                    Content = post.Content,
                    MediaUrl = post.MediaUrl ?? "",
                    MediaType = post.MediaType,
                    Status = post.Status,
                    CreatedAt = post.CreatedAt ?? now,
                    ApprovedAt = post.ApprovedAt,
                    CreatorInfo = post.User == null ? null : new PostCreatorData
                    {
                        FullName = post.User.FullName,
                        PhoneNumber = post.User.PhoneNumber,
                        City = post.User.City,
                        Sex = post.User.Sex,
                        Avatar = post.User.Avatar,
                    }
                };

                _logger.LogInformation("PostService - GetByIdAsync: Post {PostId} retrieved successfully.", postId);
                return new GeneralResult<PostDetailsDto>(true, _messages.MsgPostRetrieved, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - GetByIdAsync: Error retrieving post {PostId}.", postId);
                return new GeneralResult<PostDetailsDto>(false, _messages.GetUnexpectedErrorMessage("Get Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<PostDetailsDto>>> GetAllPublicAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var query = _dbContext.ClubPosts
                    .AsNoTracking()
                    .Where(p => p.Status == ClubPostStatus.Approved && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Include(p => p.User);

                var pagedEntities = await query.ApplyPaginationAsync(pagination, cancellationToken);
                if (!pagedEntities.Items.Any())
                {
                    _logger.LogInformation("PostService - GetAllPublicAsync: No posts found.");
                    return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.MsgNoPostsFound, null, ErrorType.NotFound);
                }

                var dtos = pagedEntities.Items.Select(post => new PostDetailsDto
                {
                    Id = post.Id,
                    Content = post.Content,
                    MediaUrl = post.MediaUrl,
                    MediaType = post.MediaType,
                    Status = post.Status,
                    CreatedAt = post.CreatedAt ?? now,
                    ApprovedAt = post.ApprovedAt,
                    CreatorInfo = post.User == null ? null : new PostCreatorData
                    {
                        FullName = post.User.FullName,
                        PhoneNumber = post.User.PhoneNumber,
                        City = post.User.City,
                        Sex = post.User.Sex,
                        Avatar = post.User.Avatar,
                    }
                }).ToList();

                var result = new PagedResult<PostDetailsDto>
                {
                    Items = dtos,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalCount = pagedEntities.TotalCount
                };

                _logger.LogInformation("PostService - GetAllPublicAsync: Retrieved {Count} posts.", dtos.Count);
                return new GeneralResult<PagedResult<PostDetailsDto>>(true, _messages.MsgPostsRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - GetAllPublicAsync: Error retrieving posts.");
                return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get Posts"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<PostDetailsDto>>> GetAllByUserAsync(string userId, PaginationRequestDto pagination, bool isAdmin, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("PostService - GetAllByUserAsync: userId is null or empty.");
                    return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                IQueryable<ClubPost>? query = null;

                if (isAdmin)
                {
                    query = _dbContext.ClubPosts
                            .AsNoTracking()
                            .Where(p => p.UserId == userId && !p.IsDeleted)
                            .OrderByDescending(p => p.CreatedAt)
                            .Include(p => p.User);
                }
                else
                {
                    query = _dbContext.ClubPosts
                            .AsNoTracking()
                            .Where(p => p.UserId == userId && !p.IsDeleted && p.Status == ClubPostStatus.Approved)
                            .OrderByDescending(p => p.CreatedAt)
                            .Include(p => p.User);
                }

                var pagedEntities = await query.ApplyPaginationAsync(pagination, cancellationToken);

                if (!pagedEntities.Items.Any())
                {
                    _logger.LogInformation("PostService - GetAllByUserAsync: No posts found for user {UserId}.", userId);
                    return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.MsgNoUserPostsFound, null, ErrorType.NotFound);
                }

                var dtos = pagedEntities.Items.Select(post => new PostDetailsDto
                {
                    Id = post.Id,
                    Content = post.Content,
                    MediaUrl = post.MediaUrl,
                    MediaType = post.MediaType,
                    Status = post.Status,
                    CreatedAt = post.CreatedAt ?? now,
                    ApprovedAt = post.ApprovedAt,
                    CreatorInfo = post.User == null ? null : new PostCreatorData
                    {
                        FullName = post.User.FullName,
                        PhoneNumber = post.User.PhoneNumber,
                        City = post.User.City,
                        Sex = post.User.Sex,
                        Avatar = post.User.Avatar,
                    }
                }).ToList();

                var result = new PagedResult<PostDetailsDto>
                {
                    Items = dtos,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalCount = pagedEntities.TotalCount
                };

                _logger.LogInformation("PostService - GetAllByUserAsync: Retrieved {Count} posts for user {UserId}.", dtos.Count, userId);
                return new GeneralResult<PagedResult<PostDetailsDto>>(true, _messages.MsgUserPostsRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - GetAllByUserAsync: Error retrieving posts for user {UserId}.", userId);
                return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get User Posts"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ReviewPostAsync(PostStatusUpdateDto dto, string adminId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                if (dto == null)
                {
                    _logger.LogWarning("PostService - ReviewPostAsync: dto is null.");
                    return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);
                }

                if (dto.PostId <= 0)
                {
                    _logger.LogWarning("PostService - ReviewPostAsync: Invalid PostId.");
                    return new GeneralResult(false, _messages.MsgPostIdInvalid, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(adminId))
                {
                    _logger.LogWarning("PostService - ReviewPostAsync: adminId is null or empty.");
                    return new GeneralResult(false, _messages.MsgAdminIdRequired, null, ErrorType.BadRequest);
                }

                var post = await _dbContext.ClubPosts.FirstOrDefaultAsync(p => p.Id == dto.PostId && !p.IsDeleted, cancellationToken);
                if (post == null)
                {
                    _logger.LogWarning("PostService - ReviewPostAsync: Post {PostId} not found.", dto.PostId);
                    return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                }

                if (post.Status != ClubPostStatus.Pending)
                {
                    _logger.LogWarning("PostService - ReviewPostAsync: Post {PostId} already reviewed.", dto.PostId);
                    return new GeneralResult(false, _messages.MsgPostAlreadyReviewed, null, ErrorType.BadRequest);
                }

                if (dto.NewStatus == ClubPostStatus.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
                {
                    _logger.LogWarning("PostService - ReviewPostAsync: Rejection reason required for rejected post.");
                    return new GeneralResult(false, _messages.MsgRejectionReasonRequired, null, ErrorType.BadRequest);
                }

                post.Status = dto.NewStatus;
                post.ApprovedAt = dto.NewStatus == ClubPostStatus.Approved ? now : null;
                post.UpdatedAt = now;
                post.ById = adminId;
                post.Note = dto.RejectionReason?.Trim() ?? "";

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PostService - ReviewPostAsync: Post {PostId} reviewed successfully.", dto.PostId);
                return new GeneralResult(true, _messages.MsgPostReviewed, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - ReviewPostAsync: Error reviewing post {PostId}.", dto?.PostId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Review Post"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<PostDetailsDto>>> GetPendingPostsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var query = _dbContext.ClubPosts
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted && p.Status == ClubPostStatus.Pending)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt);

                var pagedPosts = await query.ApplyPaginationAsync(pagination, cancellationToken);

                if (!pagedPosts.Items.Any())
                {
                    _logger.LogInformation("PostService - GetPendingPostsAsync: No pending posts found.");
                    return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.MsgNoPendingPosts, null, ErrorType.NotFound);
                }

                var result = new PagedResult<PostDetailsDto>
                {
                    Items = pagedPosts.Items.Select(p => new PostDetailsDto
                    {
                        Id = p.Id,
                        Content = p.Content,
                        MediaUrl = p.MediaUrl,
                        MediaType = p.MediaType,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt ?? now,
                        ApprovedAt = p.ApprovedAt,
                        CreatorInfo = p.User == null ? null : new PostCreatorData
                        {
                            FullName = p.User.FullName,
                            PhoneNumber = p.User.PhoneNumber,
                            City = p.User.City,
                            Sex = p.User.Sex,
                            Avatar = p.User.Avatar,
                        }
                    }).ToList(),
                    TotalCount = pagedPosts.TotalCount,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize
                };

                _logger.LogInformation("PostService - GetPendingPostsAsync: Retrieved {Count} pending posts.", result.Items.Count);
                return new GeneralResult<PagedResult<PostDetailsDto>>(true, _messages.MsgPendingPostsRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostService - GetPendingPostsAsync: Error retrieving pending posts.");
                return new GeneralResult<PagedResult<PostDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get Pending Posts"), null, ErrorType.InternalServerError);
            }
        }
    }
}
