namespace Lumora.Entities.Tables
{
    public class PodcastEpisode : SharedData
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int EpisodeNumber { get; set; }
        public string YoutubeUrl { get; set; } = null!;
        public string? ThumbnailUrl { get; set; }
    }
}
