namespace Lumora.Application.Configuration
{
    public class CookiesConfig
    {
        public bool Enable { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ExpireTime { get; set; } = 12; // Gets or sets expiration time in hours.
    }
}
