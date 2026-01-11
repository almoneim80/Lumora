namespace Lumora.Application.Configuration
{
    public class FileConfig
    {
        public string[] Extensions { get; set; } = Array.Empty<string>();

        public ExtensionConfig[] MaxSize { get; set; } = Array.Empty<ExtensionConfig>();
    }
}
