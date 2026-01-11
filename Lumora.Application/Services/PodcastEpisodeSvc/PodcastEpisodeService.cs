using Lumora.Application.Interfaces.PodcastEpisodeIntf;
namespace Lumora.Application.Services.PodcastEpisodeSvc
{
    public class PodcastEpisodeService(
        IPodcastEpisodeRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<PodcastEpisodeService> logger,
        PodcastEpisodeMessage messages) : IPodcastEpisodeService
    {
        private readonly IPodcastEpisodeRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<PodcastEpisodeService> _logger = logger;
        private readonly PodcastEpisodeMessage _messages = messages;

        public async Task<GeneralResult> CreateAsync(PodcastEpisodeCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null) return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);

                if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.YoutubeUrl))
                    return new GeneralResult(false, _messages.MsgRequiredFieldsMissing, null, ErrorType.BadRequest);

                var episode = new PodcastEpisode
                {
                    Title = dto.Title.Trim(),
                    Description = dto.Description?.Trim(),
                    EpisodeNumber = dto.EpisodeNumber ?? 0,
                    YoutubeUrl = dto.YoutubeUrl.Trim(),
                    ThumbnailUrl = dto.ThumbnailUrl ?? "",
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _repository.Add(episode);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Podcast episode created: {Title}", episode.Title);
                return new GeneralResult(true, _messages.MsgPodcastCreated, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating podcast episode.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("creating podcast episode"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> UpdateAsync(int id, PodcastEpisodeUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null) return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);

                var episode = await _repository.GetByIdAsync(id, cancellationToken);
                if (episode == null) return new GeneralResult(false, _messages.MsgEpisodeNotFound, null, ErrorType.NotFound);

                if (dto.Title != null) episode.Title = dto.Title.Trim();
                if (dto.EpisodeNumber != null) episode.EpisodeNumber = dto.EpisodeNumber ?? 0;
                if (dto.Description != null) episode.Description = dto.Description?.Trim();
                if (dto.YoutubeUrl != null) episode.YoutubeUrl = dto.YoutubeUrl.Trim();
                if (dto.ThumbnailUrl != null) episode.ThumbnailUrl = dto.ThumbnailUrl;
                episode.UpdatedAt = DateTimeOffset.UtcNow;

                _repository.Update(episode);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgPodcastUpdated, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating podcast episode {Id}", id);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating podcast episode"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var episode = await _repository.GetByIdAsync(id, cancellationToken);
                if (episode == null) return new GeneralResult(false, _messages.MsgEpisodeNotFound, null, ErrorType.NotFound);

                episode.IsDeleted = true;
                episode.UpdatedAt = DateTimeOffset.UtcNow;

                _repository.Update(episode);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgPodcastDeleted, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting podcast episode {Id}", id);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("deleting podcast episode"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<PodcastEpisodeDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var episodeDto = await _repository.GetDetailsByIdAsync(id, cancellationToken);

                if (episodeDto == null)
                    return new GeneralResult<PodcastEpisodeDetailsDto>(false, _messages.MsgEpisodeNotFound, null, ErrorType.NotFound);

                return new GeneralResult<PodcastEpisodeDetailsDto>(true, _messages.MsgPodcastFetched, episodeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving podcast episode {Id}", id);
                return new GeneralResult<PodcastEpisodeDetailsDto>(false, _messages.GetUnexpectedErrorMessage("retrieving podcast episode"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>> GetAllAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var pagedResult = await _repository.GetPagedListAsync(pagination, cancellationToken);
                return new GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>(true, _messages.MsgPodcastListFetched, pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving podcast list.");
                return new GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("retrieving podcast list"), null, ErrorType.InternalServerError);
            }
        }
    }
}
