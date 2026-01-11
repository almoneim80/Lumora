namespace Lumora.Application.Services.ClubSvc
{
    public class AmbassadorService(
            IAmbassadorRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<AmbassadorService> logger,
            ClubMessage messages) : IAmbassadorService
    {
        private readonly IAmbassadorRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<AmbassadorService> _logger = logger;
        private readonly ClubMessage _messages = messages;

        public async Task<GeneralResult> AssignAmbassadorAsync(AmbassadorAssignDto dto, string adminId, CancellationToken cancellationToken, string createdByIp, string createdByAgent)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                if (dto == null) return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);
                if (string.IsNullOrWhiteSpace(dto.UserId)) return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                if (dto.DurationInDays <= 0) return new GeneralResult(false, _messages.MsgInvalidAmbassadorDuration, null, ErrorType.BadRequest);

                var user = await _repository.GetUserForAmbassadorAsync(dto.UserId, cancellationToken);
                if (user == null) return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);

                if (await _repository.HasOverlappingAmbassadorAsync(now, cancellationToken))
                    return new GeneralResult(false, _messages.MsgAmbassadorAlreadyActive, null, ErrorType.BadRequest);

                if (dto.ClubPostId.HasValue)
                {
                    var post = await _repository.GetApprovedPostAsync(dto.ClubPostId.Value, cancellationToken);
                    if (post == null) return new GeneralResult(false, _messages.MsgPostNotFound, null, ErrorType.NotFound);
                }

                var ambassador = new ClubAmbassador
                {
                    UserId = dto.UserId,
                    ClubPostId = dto.ClubPostId,
                    StartDate = now,
                    EndDate = now.AddDays(dto.DurationInDays),
                    Reason = dto.Reason?.Trim(),
                    CreatedAt = now,
                    ById = adminId,
                    ByIp = createdByIp,
                    ByAgent = createdByAgent
                };

                await _repository.AddAsync(ambassador, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgAmbassadorAssigned, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorService - AssignAmbassadorAsync Unexpected error");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Assign Ambassador"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> RemoveAmbassadorAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                if (id <= 0) return new GeneralResult(false, _messages.MsgAmbassadorIdInvalid, null, ErrorType.BadRequest);

                var record = await _repository.GetByIdAsync(id, cancellationToken);
                if (record == null) return new GeneralResult(false, _messages.MsgAmbassadorNotFound, null, ErrorType.NotFound);

                record.EndDate = DateTimeOffset.UtcNow;
                record.UpdatedAt = DateTimeOffset.UtcNow;

                _repository.Update(record);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgAmbassadorRemoved, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorService - RemoveAmbassadorAsync Error for ID {Id}", id);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Remove Ambassador"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<AmbassadorDetailsDto?>> GetCurrentAmbassadorAsync(CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var current = await _repository.GetActiveAmbassadorAsync(now, cancellationToken);

                if (current == null) return new GeneralResult<AmbassadorDetailsDto?>(true, _messages.MsgNoActiveAmbassador, null);

                var dto = MapToDetailsDto(current, now);
                return new GeneralResult<AmbassadorDetailsDto?>(true, _messages.MsgCurrentAmbassadorRetrieved, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorService - GetCurrentAmbassadorAsync Error");
                return new GeneralResult<AmbassadorDetailsDto?>(false, _messages.GetUnexpectedErrorMessage("Get current ambassador"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<List<AmbassadorDetailsDto>>> GetAmbassadorHistoryAsync(CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var history = await _repository.GetHistoryAsync(now, cancellationToken);

                if (!history.Any()) return new GeneralResult<List<AmbassadorDetailsDto>>(true, _messages.MsgAmbassadorHistoryEmpty, []);

                var result = history.Select(a => MapToDetailsDto(a, now)).ToList();
                return new GeneralResult<List<AmbassadorDetailsDto>>(true, _messages.MsgAmbassadorHistorySuccess, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbassadorService - GetAmbassadorHistoryAsync Error");
                return new GeneralResult<List<AmbassadorDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get ambassador history"), null, ErrorType.InternalServerError);
            }
        }

        // Helper private method to maintain DRY and fix the double-assignment bug in original code
        private static AmbassadorDetailsDto MapToDetailsDto(ClubAmbassador entity, DateTimeOffset now)
        {
            return new AmbassadorDetailsDto
            {
                Id = entity.Id,
                AppointedStartDate = entity.StartDate,
                AppointedEndDate = entity.EndDate ?? now,
                AppointedReason = entity.Reason,
                AmbassadorPost = entity.ClubPost == null ? null : new AmbassadorPost
                {
                    Id = entity.ClubPost.Id,
                    Content = entity.ClubPost.Content,
                    MediaUrl = entity.ClubPost.MediaUrl,
                    MediaType = entity.ClubPost.MediaType,
                    Status = entity.ClubPost.Status,
                    CreatedAt = entity.ClubPost.CreatedAt ?? entity.StartDate,
                    ApprovedAt = entity.ClubPost.ApprovedAt,
                },
                CreatorInfo = entity.User == null ? null : new AmbassadorData
                {
                    FullName = entity.User.FullName,
                    PhoneNumber = entity.User.PhoneNumber,
                    City = entity.User.City,
                    Sex = entity.User.Sex,
                    Avatar = entity.User.Avatar,
                }
            };
        }
    }
}
