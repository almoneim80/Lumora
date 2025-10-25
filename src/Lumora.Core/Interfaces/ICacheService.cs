using Microsoft.Extensions.Caching.Memory;
namespace Lumora.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Gets the value from the cache by key.
        /// </summary>
        T? Get<T>(string key);

        /// <summary>
        /// Sets the value to the cache by key.
        /// </summary>
        void Set<T>(string key, T value, TimeSpan expirationTime, bool useSlidingExpiration = false, CacheItemPriority priority = CacheItemPriority.Normal, long? size = null);

        /// <summary>
        /// Removes the value from the cache by key.
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Checks if the value exists in the cache by key.
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Gets the value from the cache by key asynchronously.
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Sets the value to the cache by key asynchronously.
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan expirationTime, bool useSlidingExpiration = false, CacheItemPriority priority = CacheItemPriority.Normal, long? size = null);
    }
}
