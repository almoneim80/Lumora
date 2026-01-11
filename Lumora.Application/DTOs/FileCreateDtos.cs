namespace Lumora.Application.DTOs;

public class FileCreateDto
{
    [Required]
    [FileExtension]
    public IFileStream? File { get; set; }

    [Required]
    public string ScopeUid { get; set; } = string.Empty;
}
