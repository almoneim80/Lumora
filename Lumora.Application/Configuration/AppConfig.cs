namespace Lumora.Application.Configuration
{
    public class AppConfig
    {
        public PostgresConfig Postgres { get; set; } = new PostgresConfig();

        public ElasticConfig Elastic { get; set; } = new ElasticConfig();
    }
}
