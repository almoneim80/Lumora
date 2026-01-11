namespace Lumora.Application.Configuration
{
    public class CacheProfileConfig
    {
        public string Type { get; set; } = string.Empty;

        public string VaryByHeader { get; set; } = string.Empty;

        public int? DurationInDays { get; set; }
    }
}
