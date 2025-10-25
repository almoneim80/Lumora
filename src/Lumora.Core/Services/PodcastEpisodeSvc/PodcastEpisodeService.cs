using Lumora.DTOs.Podcast;
using Lumora.Extensions;
using Lumora.Interfaces.PodcastEpisodeIntf;
namespace Lumora.Services.PodcastEpisodeSvc
{
    public class PodcastEpisodeService(PgDbContext dbContext, ILogger<PodcastEpisodeService> logger, PodcastEpisodeMessage messages) : IPodcastEpisodeService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<PodcastEpisodeService> _logger = logger;
        private readonly PodcastEpisodeMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> CreateAsync(PodcastEpisodeCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                if (dto == null)
                {
                    _logger.LogWarning("PodcastEpisodeService - CreateAsync : DTO is null.");
                    return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.YoutubeUrl))
                {
                    _logger.LogWarning("PodcastEpisodeService - CreateAsync : Required fields are missing.");
                    return new GeneralResult(false, _messages.MsgRequiredFieldsMissing, null, ErrorType.BadRequest);
                }

                var episode = new PodcastEpisode
                {
                    Title = dto.Title.Trim(),
                    Description = dto.Description?.Trim(),
                    EpisodeNumber = dto.EpisodeNumber ?? 0,
                    YoutubeUrl = dto.YoutubeUrl.Trim(),
                    ThumbnailUrl = dto.ThumbnailUrl ?? "",
                    CreatedAt = now
                };

                _dbContext.PodcastEpisodes.Add(episode);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PodcastEpisodeService - CreateAsync : Podcast episode created successfully. Title={Title}", episode.Title);
                return new GeneralResult(true, _messages.MsgPodcastCreated, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PodcastEpisodeService - CreateAsync : Error creating podcast episode.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("creating podcast episode"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateAsync(int id, PodcastEpisodeUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("PodcastEpisodeService - UpdateAsync : DTO is null.");
                    return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);
                }

                var episode = await _dbContext.PodcastEpisodes.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
                if (episode == null)
                {
                    _logger.LogWarning("PodcastEpisodeService - UpdateAsync : Episode not found. Id={EpisodeId}", id);
                    return new GeneralResult(false, _messages.MsgEpisodeNotFound, null, ErrorType.NotFound);
                }

                if (dto.Title != null) episode.Title = dto.Title.Trim();
                if (dto.EpisodeNumber != null) episode.EpisodeNumber = dto.EpisodeNumber ?? 0;
                if (dto.Description != null) episode.Description = dto.Description?.Trim();
                if (dto.YoutubeUrl != null) episode.YoutubeUrl = dto.YoutubeUrl.Trim();
                if (dto.ThumbnailUrl != null) episode.ThumbnailUrl = dto.ThumbnailUrl;
                episode.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PodcastEpisodeService - UpdateAsync : Podcast episode updated successfully. Id={EpisodeId}", id);
                return new GeneralResult(true, _messages.MsgPodcastUpdated, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PodcastEpisodeService - UpdateAsync : Error updating podcast episode. Id={EpisodeId}", id);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating podcast episode"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("PodcastEpisodeService - DeleteAsync : Invalid episode ID.");
                    return new GeneralResult(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var episode = await _dbContext.PodcastEpisodes.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
                if (episode == null)
                {
                    _logger.LogWarning("PodcastEpisodeService - DeleteAsync : Episode not found. Id={EpisodeId}", id);
                    return new GeneralResult(false, _messages.MsgEpisodeNotFound, null, ErrorType.NotFound);
                }

                episode.IsDeleted = true;
                episode.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("PodcastEpisodeService - DeleteAsync : Podcast episode deleted successfully. Id={EpisodeId}", id);
                return new GeneralResult(true, _messages.MsgPodcastDeleted, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PodcastEpisodeService - DeleteAsync : Error deleting podcast episode. Id={EpisodeId}", id);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("deleting podcast episode"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PodcastEpisodeDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                if (id <= 0)
                {
                    _logger.LogWarning("PodcastEpisodeService - GetByIdAsync : Invalid episode ID.");
                    return new GeneralResult<PodcastEpisodeDetailsDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var episode = await _dbContext.PodcastEpisodes
                    .AsNoTracking().Where(e => e.Id == id && !e.IsDeleted)
                    .Select(e => new PodcastEpisodeDetailsDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        YoutubeUrl = e.YoutubeUrl,
                        ThumbnailUrl = e.ThumbnailUrl,
                        EpisodeNumber = e.EpisodeNumber,
                        CreatedAt = e.CreatedAt ?? now,
                    }).FirstOrDefaultAsync(cancellationToken);

                if (episode == null)
                {
                    _logger.LogWarning("PodcastEpisodeService - GetByIdAsync : Episode not found. Id={EpisodeId}", id);
                    return new GeneralResult<PodcastEpisodeDetailsDto>(false, _messages.MsgEpisodeNotFound, null, ErrorType.NotFound);
                }

                _logger.LogInformation("PodcastEpisodeService - GetByIdAsync : Podcast episode retrieved. Id={EpisodeId}", id);
                return new GeneralResult<PodcastEpisodeDetailsDto>(true, _messages.MsgPodcastFetched, episode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PodcastEpisodeService - GetByIdAsync : Error retrieving podcast episode. Id={EpisodeId}", id);
                return new GeneralResult<PodcastEpisodeDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving podcast episode"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>> GetAllAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                var query = _dbContext.PodcastEpisodes
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted)
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => new PodcastEpisodeDetailsDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        EpisodeNumber = e.EpisodeNumber,
                        YoutubeUrl = e.YoutubeUrl,
                        ThumbnailUrl = e.ThumbnailUrl,
                        CreatedAt = e.CreatedAt ?? now
                    });

                var pagedResult = await query.ApplyPaginationAsync(pagination, cancellationToken);

                _logger.LogInformation("PodcastEpisodeService - GetAllAsync : Retrieved {Count} episodes.", pagedResult.Items.Count);
                return new GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>(true, _messages.MsgPodcastListFetched, pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PodcastEpisodeService - GetAllAsync : Error retrieving podcast episodes.");
                return new GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("retrieving podcast list"), null, ErrorType.InternalServerError);
            }
        }
    }
}
