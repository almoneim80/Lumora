namespace Lumora.Application.Interfaces.PodcastEpisodeIntf
{
    public interface IPodcastEpisodeRepository
    {
        Task<PodcastEpisode?> GetByIdAsync(int id, CancellationToken ct);
        IQueryable<PodcastEpisode> GetQueryable();
        void Add(PodcastEpisode episode);
        void Update(PodcastEpisode episode);
        Task<PagedResult<PodcastEpisodeDetailsDto>> GetPagedListAsync(PaginationRequestDto pagination, CancellationToken ct);
        Task<PodcastEpisodeDetailsDto?> GetDetailsByIdAsync(int id, CancellationToken ct);
    }
}
