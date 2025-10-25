using Microsoft.Extensions.Caching.Memory;

namespace Lumora.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;
    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public T? Get<T>(string key)
    {
        try
        {
            _logger.LogInformation("Cache: Attempting to retrieve value for key: {Key}", key);
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return value;
            }
            else
            {
                _logger.LogInformation("Cache miss for key: {Key}", key);
                return default;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the cache for key: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public void Set<T>(string key, T value, TimeSpan expirationTime, bool useSlidingExpiration = false, CacheItemPriority priority = CacheItemPriority.Normal, long? size = null)
    {
        try
        {
            _logger.LogInformation("Cache: Setting value for key: {Key}", key);
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                Priority = priority,
            };

            if (useSlidingExpiration)
            {
                cacheEntryOptions.SlidingExpiration = expirationTime;
            }
            else
            {
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = expirationTime;
            }

            if (size.HasValue)
            {
                cacheEntryOptions.Size = size.Value;
            }

            _memoryCache.Set(key, value, cacheEntryOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while setting the cache for key: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key)
    {
        try
        {
            _logger.LogInformation("Cache: Removing value for key: {Key}", key);
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while removing the cache for key: {Key}", key);
            throw;
        } 
    }

    /// <inheritdoc/>
    public void Clear()
    {
        try
        {
            _logger.LogInformation("Cache: Clearing all cache.");
            if (_memoryCache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0); // Clear all entries
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while clearing the cache.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            _logger.LogInformation("Cache: Attempting to retrieve value for key: {Key}", key);
            var result = await Task.FromResult(Get<T>(key));
            if (result != null)
            {
                _logger.LogInformation("Cache hit for key: {Key}", key);
            }
            else
            {
                _logger.LogInformation("Cache miss for key: {Key}", key);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the cache asynchronously for key: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expirationTime, bool useSlidingExpiration = false, CacheItemPriority priority = CacheItemPriority.Normal, long? size = null)
    {
        try
        {
            _logger.LogInformation("Cache: Setting value for key: {Key}", key);
            await Task.Run(() =>
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    Priority = priority,
                };

                if (useSlidingExpiration)
                {
                    cacheEntryOptions.SlidingExpiration = expirationTime;
                }
                else
                {
                    cacheEntryOptions.AbsoluteExpirationRelativeToNow = expirationTime;
                }

                if (size.HasValue)
                {
                    cacheEntryOptions.Size = size.Value;
                }

                _memoryCache.Set(key, value, cacheEntryOptions);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while setting the cache asynchronously for key: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key)
    {
        return await Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }
}
