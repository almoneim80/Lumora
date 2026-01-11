namespace Lumora.Application.Configuration
{
    public class MediaConfig
    {
        public string[] Extensions { get; set; } = Array.Empty<string>();
        public ExtensionConfig[] MaxSize { get; set; } = Array.Empty<ExtensionConfig>();
        public string? CacheTime { get; set; }
    }
}
