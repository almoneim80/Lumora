using Lumora.Application.DTOs.Podcast;
using Lumora.Application.Interfaces.PodcastEpisodeIntf;
namespace Lumora.Infrastructure.Repositories
{
    public class PodcastEpisodeRepository(PgDbContext dbContext) : IPodcastEpisodeRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<PodcastEpisode?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _dbContext.PodcastEpisodes
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        }

        public IQueryable<PodcastEpisode> GetQueryable()
        {
            return _dbContext.PodcastEpisodes.AsNoTracking();
        }

        public void Add(PodcastEpisode episode)
        {
            _dbContext.PodcastEpisodes.Add(episode);
        }

        public void Update(PodcastEpisode episode)
        {
            _dbContext.PodcastEpisodes.Update(episode);
        }

        public async Task<PagedResult<PodcastEpisodeDetailsDto>> GetPagedListAsync(PaginationRequestDto pagination, CancellationToken ct)
        {
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
                    CreatedAt = e.CreatedAt ?? DateTimeOffset.UtcNow
                });
            return await query.ApplyPaginationAsync(pagination, ct);
        }

        public async Task<PodcastEpisodeDetailsDto?> GetDetailsByIdAsync(int id, CancellationToken ct)
        {
            return await _dbContext.PodcastEpisodes
                .AsNoTracking()
                .Where(e => e.Id == id && !e.IsDeleted)
                .Select(e => new PodcastEpisodeDetailsDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    YoutubeUrl = e.YoutubeUrl,
                    ThumbnailUrl = e.ThumbnailUrl,
                    EpisodeNumber = e.EpisodeNumber,
                    CreatedAt = e.CreatedAt ?? DateTimeOffset.UtcNow,
                }).FirstOrDefaultAsync(ct);
        }
    }
}
