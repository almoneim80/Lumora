using SurrogateIdentityAttribute = Lumora.Application.Attributes.SurrogateIdentityAttribute;

namespace Lumora.Infrastructure.Metadata
{
    public class EntityMetadataProvider : IEntityMetadataProvider
    {
        public string? GetAlternateKeyPropertyName<T>()
        {
            var uniqueIndexPropertyName = typeof(T).GetCustomAttributes(typeof(IndexAttribute), true)
                .Cast<IndexAttribute>()
                .Where(a => a.IsUnique)
                .Select(a => a.PropertyNames[0])
                .FirstOrDefault();

            if (uniqueIndexPropertyName is null)
            {
                uniqueIndexPropertyName = typeof(T).GetCustomAttributes(typeof(SurrogateIdentityAttribute), true)
                    .Cast<SurrogateIdentityAttribute>()
                    .Select(a => a.PropertyName)
                    .FirstOrDefault();
            }

            return uniqueIndexPropertyName;
        }
    }
}
