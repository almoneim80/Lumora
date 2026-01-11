namespace Lumora.Application.DTOs.MediaDtos
{
    public class ImageCreateDto
    {
        [Required]
        [MediaExtension]
        public IFileStream? Image { get; set; }

        [Required]
        public string ScopeUid { get; set; } = string.Empty;
    }
}
