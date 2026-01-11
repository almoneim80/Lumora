namespace Lumora.Application.Services.ClubSvc
{
    public class WheelAwardService(
            IWheelAwardRepository repository,
            IUnitOfWork unitOfWork,
            WheelMessag messages,
            ILogger<WheelAwardService> logger) : IWheelAwardService
    {
        private readonly IWheelAwardRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly WheelMessag _messages = messages;
        private readonly ILogger<WheelAwardService> _logger = logger;

        /// <inheritdoc/>
        public async Task<GeneralResult<WheelAwardDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("WheelAwardService - GetByIdAsync : Invalid ID={Id}", id);
                    return new GeneralResult<WheelAwardDetailsDto>(false, _messages.MsgInvalidId, null, ErrorType.BadRequest);
                }

                var award = await _repository.GetByIdAsync(id, cancellationToken);
                if (award == null)
                {
                    _logger.LogWarning("WheelAwardService - GetByIdAsync : Award not found. ID={Id}", id);
                    return new GeneralResult<WheelAwardDetailsDto>(false, _messages.MsgAwardNotFound, null, ErrorType.NotFound);
                }

                var result = new WheelAwardDetailsDto
                {
                    Id = id,
                    Name = award.Name,
                    Description = award.Description,
                    Probability = award.Probability,
                    Type = award.Type,
                    CreatedAt = award.CreatedAt,
                    UpdatedAt = award.UpdatedAt,
                };

                _logger.LogInformation("WheelAwardService - GetByIdAsync : Retrieved award with ID={Id}", id);
                return new GeneralResult<WheelAwardDetailsDto>(true, _messages.MsgAwardRetrieved, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WheelAwardService - GetByIdAsync : Unexpected error while retrieving award. ID={Id}", id);
                return new GeneralResult<WheelAwardDetailsDto>(false, _messages.GetUnexpectedErrorMessage("retrieving award"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<WheelAwardDetailsDto>>> GetAllAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var pagedEntities = await _repository.GetAllPagedAsync(pagination, cancellationToken);
                var pagedResult = new PagedResult<WheelAwardDetailsDto>
                {
                    PageSize = pagedEntities.PageSize,
                    TotalCount = pagedEntities.TotalCount,
                    Items = pagedEntities.Items.Select(x => new WheelAwardDetailsDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Probability = x.Probability,
                        Type = x.Type,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    }).ToList()
                };

                _logger.LogInformation("WheelAwardService - GetAllAsync : Retrieved {Count} wheel awards.", pagedResult.Items.Count);
                return new GeneralResult<PagedResult<WheelAwardDetailsDto>>(true, _messages.MsgWheelAwardsFetched, pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WheelAwardService - GetAllAsync : Error occurred while retrieving wheel awards.");
                return new GeneralResult<PagedResult<WheelAwardDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get All Wheel Awards"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> CreateAsync(WheelAwardCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null) return new(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);
                if (string.IsNullOrWhiteSpace(dto.Name) || dto.Probability < 0 || dto.Probability > 1)
                    return new(false, _messages.MsgRequiredFieldsInvalid, null, ErrorType.BadRequest);

                var entity = new WheelAward
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim(),
                    Probability = dto.Probability,
                    Type = dto.Type,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _repository.AddAsync(entity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new(true, _messages.MsgWheelAwardCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error CreateAsync");
                return new(false, _messages.GetUnexpectedErrorMessage("Create"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateAsync(int id, WheelAwardUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id, cancellationToken);
                if (entity == null) return new(false, _messages.MsgWheelAwardNotFound, null, ErrorType.NotFound);

                if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
                if (dto.Description != null) entity.Description = dto.Description?.Trim();
                if (dto.Probability.HasValue)
                {
                    if (dto.Probability < 0 || dto.Probability > 1) return new(false, _messages.MsgProbabilityInvalid, null, ErrorType.BadRequest);
                    entity.Probability = dto.Probability;
                }
                if (dto.Type.HasValue) entity.Type = dto.Type.Value;

                entity.UpdatedAt = DateTimeOffset.UtcNow;

                _repository.Update(entity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new(true, _messages.MsgWheelAwardUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateAsync ID={Id}", id);
                return new(false, _messages.GetUnexpectedErrorMessage("Update"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id, cancellationToken);
                if (entity == null) return new(false, _messages.MsgWheelAwardNotFound, null, ErrorType.NotFound);

                entity.IsDeleted = true;
                entity.DeletedAt = DateTimeOffset.UtcNow;

                _repository.Update(entity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new(true, _messages.MsgWheelAwardDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error DeleteAsync ID={Id}", id);
                return new(false, _messages.GetUnexpectedErrorMessage("Delete"), null, ErrorType.InternalServerError);
            }
        }
    }
}
