namespace Lumora.Services.StaticContentSvc
{
    public class StaticContentService(
        IStaticContentRepository repository, ILocalizationManager localization,
        ILogger<StaticContentService> logger,
        IStaticContentFallbackProvider fallbackProvider) : IStaticContentService
    {
        private readonly IStaticContentRepository _repository = repository;
        private readonly ILogger<StaticContentService> _logger = logger;
        private readonly ILocalizationManager _localization = localization;
        private readonly IStaticContentFallbackProvider _fallbackProvider = fallbackProvider;

        /// <inheritdoc />
        public async Task<string?> GetValueAsync(string key, string language = "ar")
        {
            var content = await _repository.GetByKeyAsync(key, language);
            return content?.Value ?? _fallbackProvider.GetDefaultValue(key, language);
        }

        /// <inheritdoc />
        public async Task<StaticContent?> GetAsync(string key, string language = "ar")
        {
            return await _repository.GetByKeyAsync(key, language);
        }

        /// <inheritdoc />
        public async Task<List<StaticContent>> GetByKeysAsync(IEnumerable<string> keys, string language = "ar")
        {
            var fromDb = await _repository.GetByKeysAsync(keys, language);
            var foundKeys = fromDb.Select(c => c.Key).ToHashSet();

            var results = new List<StaticContent>(fromDb);

            // إضافة الـ Fallback للقيم المفقودة
            foreach (var key in keys.Where(k => !foundKeys.Contains(k)))
            {
                results.Add(new StaticContent
                {
                    Key = key,
                    Value = _fallbackProvider.GetDefaultValue(key, language) ?? string.Empty,
                    Language = language,
                    IsActive = true
                });
            }
            return results.OrderBy(r => r.Key).ToList();
        }

        /// <inheritdoc />
        public async Task<List<StaticContent>> GetAllAsync(string? group = null, string? language = null, bool? isActive = true)
        {
            return await _repository.GetAllAsync(group, language, isActive);
        }

        /// <inheritdoc />
        public async Task<GeneralResult> SetValueAsync(string key, string value, string language = "ar")
        {
            var entry = await _repository.GetByKeyAsync(key, language, onlyActive: false);
            if (entry == null)
            {
                await _repository.AddAsync(new StaticContent { Key = key, Value = value, Language = language, IsActive = true, LastModified = DateTimeOffset.UtcNow });
            }
            else
            {
                entry.Value = value;
                entry.LastModified = DateTimeOffset.UtcNow;
                await _repository.UpdateAsync(entry);
            }
            await _repository.SaveChangesAsync();
            return new GeneralResult(true, _localization.GetLocalizedString("StaticContentUpdated"));
        }

        /// <inheritdoc />
        public async Task<GeneralResult> SaveAsync(StaticContentCreateDto content)
        {
            var existing = await _repository.GetByKeyAsync(content.Key ?? string.Empty, content.Language ?? "ar", onlyActive: false);

            if (existing == null)
            {
                var newEntity = new StaticContent
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
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _repository.AddAsync(newEntity);
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

                await _repository.UpdateAsync(existing);
            }

            await _repository.SaveChangesAsync();

            return new GeneralResult(true, _localization.GetLocalizedString("StaticContentSaved"));
        }

        /// <inheritdoc />
        public async Task<GeneralResult> DeleteAsync(string key, string language = "ar")
        {
            var content = await _repository.GetByKeyAsync(key, language);
            if (content == null) return new GeneralResult(false, _localization.GetLocalizedString("StaticContentNotFound"));

            content.IsActive = false;
            content.LastModified = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(content);
            await _repository.SaveChangesAsync();

            return new GeneralResult(true, _localization.GetLocalizedString("StaticContentDeactivated"));
        }
    }
}
