namespace Lumora.DTOs.Podcast;

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

    [FromForm]
    public string Title { get; set; }

    [FromForm]
    public string Description { get; set; }

    [FromForm]
    public int EpisodeNumber { get; set; }

    [FromForm]
    public string YoutubeUrl { get; set; }

    [FromForm]
    public IFormFile Thumbnail { get; set; }
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
    [FromForm]
    public string? Title { get; set; }

    [FromForm]
    public int? EpisodeNumber { get; set; }

    [FromForm]
    public string? Description { get; set; }

    [FromForm]
    public string? YoutubeUrl { get; set; }

    [FromForm]
    public IFormFile? ThumbnailFile { get; set; }
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
