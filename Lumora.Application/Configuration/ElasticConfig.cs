using Lumora.Domain.Configuration;
namespace Lumora.Application.Configuration
{
    public class ElasticConfig : BaseServiceConfig
    {
        public bool UseHttps { get; set; } = false;

        public string IndexPrefix { get; set; } = string.Empty;

        public string Url => $"http{(UseHttps ? "s" : string.Empty)}://{Server}:{Port}";
    }
}
