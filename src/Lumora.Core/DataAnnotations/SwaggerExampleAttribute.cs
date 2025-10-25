namespace Lumora.DataAnnotations;

public class SwaggerExampleAttribute<T> : Attribute
{
    public SwaggerExampleAttribute(T value)
    {
        Value = value;
    }

    public T Value { get; }
}
