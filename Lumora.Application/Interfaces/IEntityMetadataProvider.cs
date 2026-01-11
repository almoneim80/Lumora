namespace Lumora.Application.Interfaces
{
    public interface IEntityMetadataProvider
    {
        string? GetAlternateKeyPropertyName<T>();
    }
}
