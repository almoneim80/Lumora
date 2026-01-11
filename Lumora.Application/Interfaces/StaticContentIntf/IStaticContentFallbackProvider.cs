namespace Lumora.Application.Interfaces.StaticContentIntf
{
    public interface IStaticContentFallbackProvider
    {
        string? GetDefaultValue(string key, string language);
    }
}
