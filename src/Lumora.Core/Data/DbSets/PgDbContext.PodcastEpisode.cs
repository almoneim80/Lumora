namespace Lumora.Data
{
    /// <summary>
    /// PodcastEpisode DbSet.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<PodcastEpisode> PodcastEpisodes { get; set; } = null!;
    }
}
