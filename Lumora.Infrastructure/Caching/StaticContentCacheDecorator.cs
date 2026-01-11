namespace Lumora.Infrastructure.Caching
{
    public class StaticContentCacheDecorator(
            IStaticContentService innerService,
            ICacheService cache) : IStaticContentService
    {
        private readonly IStaticContentService _innerService = innerService;
        private readonly ICacheService _cache = cache;

        public async Task<string?> GetValueAsync(string key, string language = "ar")
        {
            var cacheKey = $"StaticContent:{language}:{key}";

            // محاولة جلب البيانات من الكاش
            var cached = await _cache.GetAsync<string>(cacheKey);
            if (cached != null) return cached;

            // إذا لم توجد، نذهب للخدمة الأساسية (التي تذهب للمستودع)
            var value = await _innerService.GetValueAsync(key, language);

            if (value != null)
            {
                // إصلاح الاستدعاء بتمرير المعاملات المطلوبة حسب الواجهة
                await _cache.SetAsync(cacheKey, value, TimeSpan.FromMinutes(30));
            }

            return value;
        }

        public async Task<GeneralResult> SetValueAsync(string key, string value, string language = "ar")
        {
            var result = await _innerService.SetValueAsync(key, value, language);

            // في حال النجاح، يجب مسح الكاش القديم لضمان عدم قراءة بيانات منتهية
            if (result.IsSuccess == true)
            {
                await _cache.RemoveAsync($"StaticContent:{language}:{key}");
            }
            return result;
        }

        public async Task<GeneralResult> SaveAsync(StaticContentCreateDto content)
        {
            var result = await _innerService.SaveAsync(content);
            if (result.IsSuccess == true)
            {
                await _cache.RemoveAsync($"StaticContent:{content.Language}:{content.Key}");
            }
            return result;
        }

        public async Task<GeneralResult> DeleteAsync(string key, string language = "ar")
        {
            var result = await _innerService.DeleteAsync(key, language);
            if (result.IsSuccess == true)
            {
                await _cache.RemoveAsync($"StaticContent:{language}:{key}");
            }

            return result;
        }

        // تمرير الدوال التي لا تحتاج كاش حالياً للخدمة الداخلية
        public Task<StaticContent?> GetAsync(string key, string language = "ar") => _innerService.GetAsync(key, language);
        public Task<List<StaticContent>> GetByKeysAsync(IEnumerable<string> keys, string language = "ar") => _innerService.GetByKeysAsync(keys, language);
        public Task<List<StaticContent>> GetAllAsync(string? group = null, string? language = null, bool? isActive = true) => _innerService.GetAllAsync(group, language, isActive);
    }
}
