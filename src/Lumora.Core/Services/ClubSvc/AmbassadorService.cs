using Lumora.DTOs.Authentication;
using Lumora.DTOs.Club;
using Lumora.Interfaces.ClubIntf;
namespace Lumora.Services.ClubSvc
{
    public class AmbassadorService(PgDbContext dbContext, ILogger<AmbassadorService> logger, ClubMessage messages, IHttpContextHelper httpContextHelper) : IAmbassadorService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<AmbassadorService> _logger = logger;
        private readonly ClubMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> AssignAmbassadorAsync(AmbassadorAssignDto dto, string adminId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                _logger.LogWarning("NOW is: {Now}", now);
                if (dto == null)
                {
                    _logger.LogWarning("AmbassadorService - AssignAmbassadorAsync: DTO is null.");
                    return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(dto.UserId))
                {
                    return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                if (dto.DurationInDays <= 0)
                {
                    return new GeneralResult(false, _messages.MsgInvalidAmbassadorDuration, null, ErrorType.BadRequest);
                }

                var start = now;
                var end = now.AddDays(dto.DurationInDays);

                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == dto.UserId && !u.IsDeleted, cancellationToken);

                if (user == null)
                {
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var overlappingAmbassador = await _dbContext.ClubAmbassadors
                    .AnyAsync(a =>
                        now >= a.StartDate && now <= a.EndDate, cancellationToken);

                if (overlappingAmbassador)
                {
                    return new GeneralResult(false, _messages.MsgAmbassadorAlreadyActive, null, ErrorType.BadRequest);
                }

                ClubPost? post = null;
                if (dto.ClubPostId.HasValue)
                {
                    post = await _dbContext.ClubPosts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == dto.ClubPostId && !p.IsDeleted && p.Status == ClubPostStatus.Approved, cancellationToken);

                    if (post == null)
                    {
                        return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                    }
                }

                var ambassador = new ClubAmbassador
                {
                    UserId = dto.UserId,
                    ClubPostId = dto.ClubPostId,
                    StartDate = start,
                    EndDate = end,
                    Reason = dto.Reason?.Trim(),
                    CreatedAt = now,
                    ById = adminId,
                    ByIp = httpContextHelper.IpAddress,
                    ByAgent = httpContextHelper.UserAgent
                };

                await _dbContext.ClubAmbassadors.AddAsync(ambassador, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("AmbassadorService - AssignAmbassadorAsync: Ambassador assigned successfully. ID: {AmbassadorId}", ambassador.Id);
                return new GeneralResult(true, _messages.MsgAmbassadorAssigned, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorService - AssignAmbassadorAsync: Unexpected error.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Assign Ambassador"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> RemoveAmbassadorAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                if (id <= 0)
                {
                    return new GeneralResult(false, _messages.MsgAmbassadorIdInvalid, null, ErrorType.BadRequest);
                }

                var record = await _dbContext.ClubAmbassadors
                    .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

                if (record == null)
                {
                    return new GeneralResult(false, _messages.MsgAmbassadorNotFound, null, ErrorType.NotFound);
                }

                record.EndDate = now;
                record.UpdatedAt = now;
                _dbContext.ClubAmbassadors.Update(record);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("AmbassadorService - RemoveAmbassadorAsync: Ambassador with ID {Id} removed successfully.", id);
                return new GeneralResult(true, _messages.MsgAmbassadorRemoved, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorService - RemoveAmbassadorAsync: Unexpected error while removing ambassador ID {Id}.", id);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Remove Ambassador"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<AmbassadorDetailsDto?>> GetCurrentAmbassadorAsync(CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                var current = await _dbContext.ClubAmbassadors
                    .Include(a => a.ClubPost)
                    .Where(a => a.StartDate <= now && a.EndDate >= now)
                    .OrderByDescending(a => a.StartDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (current == null)
                {
                    _logger.LogInformation("AmbassadorService - GetCurrentAmbassadorAsync: No active ambassador found.");
                    return new GeneralResult<AmbassadorDetailsDto?>(true, _messages.MsgNoActiveAmbassador, null);
                }

                AmbassadorDetailsDto? ambassadorDto = null;
                if (current.ClubPost != null)
                {
                    ambassadorDto = new AmbassadorDetailsDto
                    {
                        Id = current.Id,
                        AppointedStartDate = current.StartDate,
                        AppointedEndDate = current.EndDate ?? now,
                        AppointedReason = current.Reason,
                        AmbassadorPost = new AmbassadorPost
                        {
                            Id = current.ClubPost.Id,
                            Content = current.ClubPost.Content,
                            MediaUrl = current.ClubPost.MediaUrl,
                            MediaType = current.ClubPost.MediaType,
                            Status = current.ClubPost.Status,
                            CreatedAt = current.ClubPost.CreatedAt ?? now,
                            ApprovedAt = current.ClubPost.ApprovedAt,
                        },
                        CreatorInfo = current.User == null ? null : new AmbassadorData
                        {
                            FullName = current.User.FullName,
                            PhoneNumber = current.User.PhoneNumber,
                            City = current.User.City,
                            Sex = current.User.Sex,
                            Avatar = current.User.Avatar,
                        }
                    };
                }

                ambassadorDto = new AmbassadorDetailsDto
                {
                    Id = current.Id,
                    AppointedStartDate = current.StartDate,
                    AppointedEndDate = current.EndDate ?? now,
                    AppointedReason = current.Reason,
                    AmbassadorPost = null,
                    CreatorInfo = current.User == null ? null : new AmbassadorData
                    {
                        FullName = current.User.FullName,
                        PhoneNumber = current.User.PhoneNumber,
                        City = current.User.City,
                        Sex = current.User.Sex,
                        Avatar = current.User.Avatar,
                    }
                };

                _logger.LogInformation("AmbassadorService - GetCurrentAmbassadorAsync: Current ambassador retrieved.");
                return new GeneralResult<AmbassadorDetailsDto?>(true, _messages.MsgCurrentAmbassadorRetrieved, ambassadorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorService - GetCurrentAmbassadorAsync: Unexpected error.");
                return new GeneralResult<AmbassadorDetailsDto?>(false, _messages.GetUnexpectedErrorMessage("Get current ambassador"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<AmbassadorDetailsDto>>> GetAmbassadorHistoryAsync(CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                var history = await _dbContext.ClubAmbassadors
                    .Include(a => a.ClubPost)
                    .Include(a => a.User)
                    .Where(a => a.EndDate != null && a.EndDate <= now)
                    .OrderByDescending(a => a.StartDate)
                    .ToListAsync(cancellationToken);

                if (!history.Any())
                {
                    _logger.LogInformation("AmbassadorService - GetAmbassadorHistoryAsync: No ambassador history found.");
                    return new GeneralResult<List<AmbassadorDetailsDto>>(true, _messages.MsgAmbassadorHistoryEmpty, []);
                }

                var result = history.Select(a => new AmbassadorDetailsDto
                {
                    Id = a.Id,
                    AppointedStartDate = a.StartDate,
                    AppointedEndDate = a.EndDate ?? now,
                    AppointedReason = a.Reason,
                    AmbassadorPost = a.ClubPost == null ? null : new AmbassadorPost
                    {
                        Id = a.ClubPost.Id,
                        Content = a.ClubPost.Content,
                        MediaUrl = a.ClubPost.MediaUrl,
                        MediaType = a.ClubPost.MediaType,
                        Status = a.ClubPost.Status,
                        CreatedAt = a.ClubPost.CreatedAt ?? a.StartDate,
                        ApprovedAt = a.ClubPost.ApprovedAt,
                    },
                    CreatorInfo = a.User == null ? null : new AmbassadorData
                    {
                        FullName = a.User.FullName,
                        PhoneNumber = a.User.PhoneNumber,
                        City = a.User.City,
                        Sex = a.User.Sex,
                        Avatar = a.User.Avatar,
                    }
                }).ToList();

                _logger.LogInformation("AmbassadorService - GetAmbassadorHistoryAsync: Retrieved {Count} ambassadors.", result.Count);
                return new GeneralResult<List<AmbassadorDetailsDto>>(true, _messages.MsgAmbassadorHistorySuccess, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorService - GetAmbassadorHistoryAsync: Unexpected error.");
                return new GeneralResult<List<AmbassadorDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get ambassador history"), null, ErrorType.InternalServerError);
            }
        }
    }
}
