using Lumora.Infrastructure.StaticContents;

namespace Lumora.Infrastructure.Services.ExternalServices.StaticContent
{
    public class StaticContentFallbackProvider : IStaticContentFallbackProvider
    {
        public string? GetDefaultValue(string key, string language)
        {
            return StaticContentDefaults.Get(key, language);
        }
    }
}
