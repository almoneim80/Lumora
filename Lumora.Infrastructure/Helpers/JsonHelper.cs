using Lumora.Infrastructure.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lumora.Infrastructure.Helpers
{
    public class JsonHelper
    {
        protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions();

        static JsonHelper()
        {
            Configure(SerializeOptions);
        }

        public static void Configure(JsonSerializerOptions options, JsonNamingPolicy policy)
        {
            options.PropertyNamingPolicy = policy;

            options.Converters.Add(new JsonStringEnumConverter());
        }

        public static void Configure(JsonSerializerOptions options, JsonNamingConvention convention = JsonNamingConvention.CamelCase)
        {
            if (convention == JsonNamingConvention.CamelCase)
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            }
            else
            {
                options.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
            }

            options.Converters.Add(new JsonStringEnumConverter());
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        }

        public static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, SerializeOptions);
        }

        public static T? Deserialize<T>(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return default(T);
            }
            else
            {
                return JsonSerializer.Deserialize<T>(data, SerializeOptions);
            }
        }
    }
}
