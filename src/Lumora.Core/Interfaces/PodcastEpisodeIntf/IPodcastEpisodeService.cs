using Lumora.DTOs.Podcast;

namespace Lumora.Interfaces.PodcastEpisodeIntf
{
    public interface IPodcastEpisodeService
    {
        /// <summary>
        /// Creates a new podcast episode.
        /// </summary>
        Task<GeneralResult> CreateAsync(PodcastEpisodeCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing podcast episode by ID.
        /// </summary>
        Task<GeneralResult> UpdateAsync(int id, PodcastEpisodeUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Soft deletes a podcast episode by ID.
        /// </summary>
        Task<GeneralResult> DeleteAsync(int id, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves details of a specific podcast episode by ID.
        /// </summary>
        Task<GeneralResult<PodcastEpisodeDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all podcast episodes.
        /// </summary>
        Task<GeneralResult<PagedResult<PodcastEpisodeDetailsDto>>> GetAllAsync(PaginationRequestDto pagination, CancellationToken cancellationToken);
    }
}
