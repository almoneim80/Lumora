using Microsoft.Extensions.Caching.Memory;
namespace Lumora.Application.Configuration
{
    public class CacheSettings
    {
        public int CacheExpirationMinuteشs { get; set; }
        public CacheItemPriority CacheItemPriority { get; set; } = CacheItemPriority.Normal;
        public long? CacheItemSize { get; set; }
    }
}
