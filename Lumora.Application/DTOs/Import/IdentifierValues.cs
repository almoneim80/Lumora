namespace Lumora.Application.DTOs.Import
{
    internal class IdentifierValues : Dictionary<string, List<object>>
    {
        public List<string> IdentifierPropertyNames { get; set; } = new List<string>();

        public List<string> SurrogateKeyPropertyNames { get; set; } = new List<string>();

        public List<SurrogateForeignKeyAttribute> SurrogateKeyPropertyAttributes { get; set; } = new List<SurrogateForeignKeyAttribute>();
    }
}
