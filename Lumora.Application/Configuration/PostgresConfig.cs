using Lumora.Domain.Configuration;

namespace Lumora.Application.Configuration
{
    public class PostgresConfig : BaseServiceConfig
    {
        public string Database { get; set; } = string.Empty;

        public string ConnectionString => $"User ID={UserName};Password={Password};Server={Server};Port={Port};Database={Database};Pooling=true;";
    }
}
