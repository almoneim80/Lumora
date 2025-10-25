namespace Lumora.Web.Extensions
{
    public static class LoggingExtensions
    {
        // Configure logging
        public static void ConfigureLogs(this WebApplicationBuilder builder)
        {
            var elasticConfig = builder.Configuration.GetSection("Elastic").Get<ElasticConfig>();

            if (elasticConfig == null || string.IsNullOrEmpty(elasticConfig.Server))
            {
                Console.WriteLine("ElasticSearch configuration is missing. Falling back to console logging only.");
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.Console()
                    .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(ConfigureELK(elasticConfig))
                    .CreateLogger();
            }

            builder.Host.UseSerilog();
        }

        // Configure ELK
        private static ElasticsearchSinkOptions ConfigureELK(ElasticConfig elasticConfig)
        {
            var uri = new Uri(elasticConfig.Url);

            return new ElasticsearchSinkOptions(uri)
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                IndexFormat = $"{elasticConfig.IndexPrefix}-logs",
                ModifyConnectionSettings = x => x.BasicAuthentication(elasticConfig.UserName, elasticConfig.Password).ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true),
                FailureCallback = (logEvent, exception) => Console.WriteLine($"Failed to send log to Elasticsearch: {exception.Message}"),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog
            };
        }
    }
}
