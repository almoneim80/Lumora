using Microsoft.AspNetCore.Http.HttpResults;
using Nest;
using Lumora.DTOs.StaticContent;
using Lumora.Infrastructure.StaticContentInfra;
using Lumora.Interfaces.StaticContentIntf;

namespace Lumora.Services.StaticContentSvc
{
    public class StaticContentService(
        PgDbContext dbContext, ILocalizationManager localization, ILogger<StaticContentService> logger, ICacheService cache) : IStaticContentService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<StaticContentService> _logger = logger;
        private readonly ILocalizationManager _localization = localization;
        private readonly ICacheService _cache = cache;

        /// <inheritdoc />
        public async Task<string?> GetValueAsync(string key, string language = "ar")
        {
            var cacheKey = $"StaticContent:{language}:{key}";
            var cached = await _cache.GetAsync<string>(cacheKey);
            if (!string.IsNullOrEmpty(cached))
                return cached;

            try
            {
                var value = await _dbContext.StaticContents
                    .Where(c => c.Key == key && c.Language == language && c.IsActive)
                    .Select(c => c.Value)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(value))
                {
                    await _cache.SetAsync(cacheKey, value, TimeSpan.FromMinutes(30));
                    return value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load static content for key: {key}");
            }

            // fallback
            var fallback = StaticContentDefaults.Get(key, language);
            if (!string.IsNullOrEmpty(fallback))
                await _cache.SetAsync(cacheKey, fallback, TimeSpan.FromMinutes(10));

            return fallback;
        }

        /// <inheritdoc />
        public async Task<StaticContent?> GetAsync(string key, string language = "ar")
        {
            return await _dbContext.StaticContents
                .FirstOrDefaultAsync(c => c.Key == key && c.Language == language && c.IsActive);
        }

        /// <inheritdoc />
        public async Task<List<StaticContent>> GetByKeysAsync(IEnumerable<string> keys, string language = "ar")
        {
            var result = new List<StaticContent>();
            var keysToFetch = new List<string>();
            var cacheHits = new List<StaticContent>();

            foreach (var key in keys)
            {
                var cacheKey = $"StaticContent:{language}:{key}";
                var cachedValue = await _cache.GetAsync<string>(cacheKey);
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    cacheHits.Add(new StaticContent
                    {
                        Key = key,
                        Value = cachedValue,
                        Language = language,
                        IsActive = true,
                        LastModified = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    keysToFetch.Add(key);
                }
            }

            if (keysToFetch.Any())
            {
                var fromDb = await _dbContext.StaticContents
                    .Where(c => keysToFetch.Contains(c.Key) && c.Language == language && c.IsActive)
                    .ToListAsync();

                // add results to cache
                foreach (var item in fromDb)
                {
                    var cacheKey = $"StaticContent:{language}:{item.Key}";
                    await _cache.SetAsync(cacheKey, item.Value, TimeSpan.FromMinutes(30));
                }

                // detect any still-missing keys
                var foundKeys = fromDb.Select(c => c.Key).ToHashSet();
                var fallback = keysToFetch
                    .Where(k => !foundKeys.Contains(k))
                    .Select(k => new StaticContent
                    {
                        Key = k,
                        Value = StaticContentDefaults.Get(k, language) ?? string.Empty,
                        Language = language,
                        IsActive = true,
                        LastModified = DateTimeOffset.UtcNow
                    })
                    .ToList();

                // cache fallback too
                foreach (var item in fallback.Where(f => !string.IsNullOrEmpty(f.Value)))
                {
                    var cacheKey = $"StaticContent:{language}:{item.Key}";
                    await _cache.SetAsync(cacheKey, item.Value, TimeSpan.FromMinutes(10));
                }

                result.AddRange(fromDb);
                result.AddRange(fallback);
            }

            result.AddRange(cacheHits);
            return result.OrderBy(r => r.Key).ToList();
        }

        /// <inheritdoc />
        public async Task<List<StaticContent>> GetAllAsync(string? group = null, string? language = null, bool? isActive = true)    
        {
            var query = _dbContext.StaticContents.AsQueryable();

            if (!string.IsNullOrEmpty(group))
                query = query.Where(c => c.Group == group);

            if (!string.IsNullOrEmpty(language))
                query = query.Where(c => c.Language == language);

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive);

            return await query.OrderBy(c => c.Key).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<GeneralResult> SetValueAsync(string key, string value, string language = "ar")
        {
            var entry = await _dbContext.StaticContents
                .FirstOrDefaultAsync(c => c.Key == key && c.Language == language);

            if (entry == null)
            {
                entry = new StaticContent
                {
                    Key = key,
                    Value = value,
                    Language = language,
                    LastModified = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                _dbContext.StaticContents.Add(entry);
            }
            else
            {
                entry.Value = value;
                entry.LastModified = DateTimeOffset.UtcNow;
                _dbContext.StaticContents.Update(entry);
            }

            await _dbContext.SaveChangesAsync();

            // update cache
            var cacheKey = $"StaticContent:{language}:{key}";
            await _cache.SetAsync(cacheKey, value, TimeSpan.FromMinutes(30));

            return new GeneralResult(true, _localization.GetLocalizedString("StaticContentUpdated"));
        }

        /// <inheritdoc />
        public async Task<GeneralResult> SaveAsync(StaticContentCreateDto content)
        {
            var existing = await _dbContext.StaticContents
                .FirstOrDefaultAsync(c => c.Key == content.Key && c.Language == content.Language);

            if (existing == null)
            {
                var dto = new StaticContent
                {
                    Key = content.Key,
                    Value = content.Value,
                    Language = content.Language,
                    Group = content.Group,
                    ContentType = content.ContentType,
                    MediaUrl = content.MediaUrl,
                    MediaAlt = content.MediaAlt,
                    MediaType = content.MediaType,
                    Note = content.Note,
                    CreatedAt = DateTimeOffset.UtcNow,
                };

                _dbContext.StaticContents.Add(dto);
            }
            else
            {
                existing.Value = content.Value;
                existing.Group = content.Group;
                existing.ContentType = content.ContentType;
                existing.MediaUrl = content.MediaUrl;
                existing.MediaAlt = content.MediaAlt;
                existing.MediaType = content.MediaType;
                existing.Note = content.Note;
                existing.CreatedAt = DateTimeOffset.UtcNow;
                existing.LastModified = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            // update cache
            var cacheKey = $"StaticContent:{content.Language}:{content.Key}";
            await _cache.SetAsync(cacheKey, content.Value, TimeSpan.FromMinutes(30));

            return new GeneralResult(true, _localization.GetLocalizedString("StaticContentSaved"));
        }

        /// <inheritdoc />
        public async Task<GeneralResult> DeleteAsync(string key, string language = "ar")
        {
            var content = await _dbContext.StaticContents
                .FirstOrDefaultAsync(c => c.Key == key && c.Language == language);

            if (content == null)
                return new GeneralResult(false, _localization.GetLocalizedString("StaticContentNotFound"));

            content.IsActive = false;
            content.LastModified = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();

            // update cache
            var cacheKey = $"StaticContent:{language}:{key}";
            await _cache.RemoveAsync(cacheKey);

            return new GeneralResult(true, _localization.GetLocalizedString("StaticContentDeactivated"));
        }
    }
}
