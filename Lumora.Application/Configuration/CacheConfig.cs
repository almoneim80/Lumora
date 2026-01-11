using Microsoft.Extensions.Caching.Memory;

namespace Lumora.Application.Configuration
{
    public class CacheConfig
    {
        public int CacheExpirationMinutes { get; set; }
        public CacheItemPriority CacheItemPriority { get; set; } = CacheItemPriority.Normal;
        public long? CacheItemSize { get; set; }
    }
}
