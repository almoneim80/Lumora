namespace Lumora.Application.DTOs.StaticContent;

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
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? Language { get; set; }
    public string? Group { get; set; }
    public StaticContentType ContentType { get; set; }
    public StaticContentMediaType MediaType { get; set; }
    public string? MediaAlt { get; set; }
    public string? Note { get; set; }
    public IFileStream? MediaFile { get; set; }
}
