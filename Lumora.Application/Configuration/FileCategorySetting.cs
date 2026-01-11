namespace Lumora.Application.Configuration
{
    public class FileCategorySetting
    {
        public List<string>? Extensions { get; set; }
        public List<string>? MimeTypes { get; set; }
        public Dictionary<string, string>? MaxSizePerExtension { get; set; }
    }
}
