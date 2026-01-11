namespace Lumora.Application.DTOs.MediaDtos;

public class FileCreateDto
{
    [Required]
    [FileExtension]
    public IFileStream? File { get; set; }

    [Required]
    public string ScopeUid { get; set; } = string.Empty;
}
