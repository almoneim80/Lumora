namespace Lumora.DTOs.StaticContent;

public class StaticContentCreateDto
{
    public string? Key { get; set; } 
    public string? Value { get; set; }
    public string? Language { get; set; }
    public string? Group { get; set; }
    public StaticContentType ContentType { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaAlt { get; set; }
    public StaticContentMediaType MediaType { get; set; }
    public string? Note { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
}

public class StaticContentCreateFormDto
{
    [FromForm]
    public string? Key { get; set; }

    [FromForm]
    public string? Value { get; set; }

    [FromForm]
    public string? Language { get; set; }

    [FromForm]
    public string? Group { get; set; }

    [FromForm]
    public StaticContentType ContentType { get; set; }

    [FromForm]
    public StaticContentMediaType MediaType { get; set; }

    [FromForm]
    public string? MediaAlt { get; set; }

    [FromForm]
    public string? Note { get; set; }

    [FromForm(Name = "mediaFile")]
    public IFormFile? MediaFile { get; set; }
}
