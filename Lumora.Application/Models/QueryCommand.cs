namespace Lumora.Infrastructure
{
    public class QueryCommand
    {
        public static Dictionary<string, FilterType> FilterMappings { get; } = new Dictionary<string, FilterType>()
        {
            { "field", FilterType.Fields },
            { "limit", FilterType.Limit },
            { "order", FilterType.Order },
            { "skip", FilterType.Skip },
            { "where", FilterType.Where },
            { "include", FilterType.Include },
        };

        public static Dictionary<string, WOperand> OperandMappings { get; } = new Dictionary<string, WOperand>()
        {
            { "eq", WOperand.Equal },
            { "gt", WOperand.GreaterThan },
            { "gte", WOperand.GreaterThanOrEqualTo },
            { "lt", WOperand.LessThan },
            { "lte", WOperand.LessThanOrEqualTo },
            { "neq", WOperand.NotEqual },
            { "like", WOperand.Like },
            { "nlike", WOperand.NLike },
            { "contains", WOperand.Contains },
            { "ncontains", WOperand.NContains },
        };

        public static string AvailableCommandString => FilterMappings.Keys.Aggregate(string.Empty, (acc, key) => $"{acc}{key}, ");

        public FilterType Type { get; set; } = FilterType.None;

        public string[] Props { get; set; } = Array.Empty<string>();

        public string Value { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public System.Reflection.PropertyInfo ParseProperty<T>()
        {
            var propertyName = Props.ElementAtOrDefault(0);
            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
            {
                throw new QueryException(Source, "Property field not found");
            }

            var typeProperties = typeof(T).GetProperties();
            var property = typeProperties.FirstOrDefault(p => p.Name.ToLowerInvariant() == propertyName.ToLowerInvariant());

            if (property == null)
            {
                throw new QueryException(Source, $"No such property '{propertyName}'");
            }

            return property;
        }
    }
}
