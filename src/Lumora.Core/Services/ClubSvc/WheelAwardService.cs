using Lumora.DTOs.Club;
using Lumora.Extensions;
using Lumora.Interfaces.ClubIntf;

namespace Lumora.Services.ClubSvc
{
    public class WheelAwardService(PgDbContext dbContext, WheelMessag messages, ILogger<WheelAwardService> logger) : IWheelAwardService
    {
        private readonly PgDbContext _dbContext = dbContext;
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

                var award = await _dbContext.WheelAwards
                    .AsNoTracking()
                    .Where(a => a.Id == id && !a.IsDeleted)
                    .Select(a => new WheelAwardDetailsDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        Probability = a.Probability,
                        Type = a.Type,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    }).FirstOrDefaultAsync(cancellationToken);

                if (award == null)
                {
                    _logger.LogWarning("WheelAwardService - GetByIdAsync : Award not found. ID={Id}", id);
                    return new GeneralResult<WheelAwardDetailsDto>(false, _messages.MsgAwardNotFound, null, ErrorType.NotFound);
                }

                _logger.LogInformation("WheelAwardService - GetByIdAsync : Retrieved award with ID={Id}", id);
                return new GeneralResult<WheelAwardDetailsDto>(true, _messages.MsgAwardRetrieved, award);
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
                var query = _dbContext.WheelAwards
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new WheelAwardDetailsDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Probability = x.Probability,
                        Type = x.Type,
                        CreatedAt = x.CreatedAt!.Value,
                        UpdatedAt = x.UpdatedAt
                    });

                var pagedResult = await query.ApplyPaginationAsync(pagination, cancellationToken);

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
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("WheelAwardService - CreateAsync : DTO is null.");
                    return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(dto.Name) || dto.Probability == null || dto.Probability < 0 || dto.Probability > 1)
                {
                    _logger.LogWarning("WheelAwardService - CreateAsync : Required fields are missing or invalid.");
                    return new GeneralResult(false, _messages.MsgRequiredFieldsInvalid, null, ErrorType.BadRequest);
                }

                var entity = new WheelAward
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim(),
                    Probability = dto.Probability,
                    Type = dto.Type,
                    CreatedAt = now
                };

                await _dbContext.WheelAwards.AddAsync(entity, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("WheelAwardService - CreateAsync : Wheel award created successfully. Name={Name}", entity.Name);
                return new GeneralResult(true, _messages.MsgWheelAwardCreated, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WheelAwardService - CreateAsync : Error occurred while creating wheel award.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Create Wheel Award"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateAsync(int id, WheelAwardUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("WheelAwardService - UpdateAsync : DTO is null.");
                    return new GeneralResult(false, _messages.MsgNullOrEmpty, null, ErrorType.BadRequest);
                }

                var entity = await _dbContext.WheelAwards.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);
                if (entity == null)
                {
                    _logger.LogWarning("WheelAwardService - UpdateAsync : Wheel award not found. Id={WheelAwardId}", id);
                    return new GeneralResult(false, _messages.MsgWheelAwardNotFound, null, ErrorType.NotFound);
                }

                if (!string.IsNullOrWhiteSpace(dto.Name))
                    entity.Name = dto.Name.Trim();

                if (dto.Description != null)
                    entity.Description = dto.Description?.Trim();

                if (dto.Probability.HasValue)
                {
                    if (dto.Probability < 0 || dto.Probability > 1)
                    {
                        _logger.LogWarning("WheelAwardService - UpdateAsync : Invalid probability value.");
                        return new GeneralResult(false, _messages.MsgProbabilityInvalid, null, ErrorType.BadRequest);
                    }
                    entity.Probability = dto.Probability;
                }

                if (dto.Type.HasValue)
                    entity.Type = dto.Type.Value;

                entity.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("WheelAwardService - UpdateAsync : Wheel award updated successfully. Id={WheelAwardId}", id);
                return new GeneralResult(true, _messages.MsgWheelAwardUpdated, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WheelAwardService - UpdateAsync : Error occurred while updating wheel award. Id={WheelAwardId}", id);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Update Wheel Award"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (id <= 0)
                {
                    _logger.LogWarning("WheelAwardService - DeleteAsync : Invalid ID provided.");
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var entity = await _dbContext.WheelAwards.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);
                if (entity == null)
                {
                    _logger.LogWarning("WheelAwardService - DeleteAsync : Wheel award not found. Id={WheelAwardId}", id);
                    return new GeneralResult(false, _messages.MsgWheelAwardNotFound, null, ErrorType.NotFound);
                }

                entity.IsDeleted = true;
                entity.DeletedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("WheelAwardService - DeleteAsync : Wheel award deleted successfully. Id={WheelAwardId}", id);
                return new GeneralResult(true, _messages.MsgWheelAwardDeleted, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WheelAwardService - DeleteAsync : Error occurred while deleting wheel award. Id={WheelAwardId}", id);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Delete Wheel Award"), null, ErrorType.InternalServerError);
            }
        }
    }
}
