namespace Lumora.Application.DTOs.Podcast;

public class PodcastEpisodeCreateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? EpisodeNumber { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class PodcastEpisodeCreateFormDto
{
#nullable disable
    public string Title { get; set; }
    public string Description { get; set; }
    public int EpisodeNumber { get; set; }
    public string YoutubeUrl { get; set; }
    public IFileStream Thumbnail { get; set; }
}

public class PodcastEpisodeUpdateDto
{
#nullable enable
    public string? Title { get; set; }
    public int? EpisodeNumber { get; set; }
    public string? Description { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class PodcastEpisodeUpdateFormDto
{
#nullable enable
    public string? Title { get; set; }
    public int? EpisodeNumber { get; set; }
    public string? Description { get; set; }
    public string? YoutubeUrl { get; set; }
    public IFileStream? ThumbnailFile { get; set; }
}


public class PodcastEpisodeDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int EpisodeNumber { get; set; }
    public string YoutubeUrl { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
