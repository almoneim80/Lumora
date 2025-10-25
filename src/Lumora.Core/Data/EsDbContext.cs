using Lumora.Elastic;

namespace Lumora.Data;

/// <summary>
/// This is a practical implementation of ElasticDbContext.
/// </summary>
public class EsDbContext : ElasticDbContext
{
    private readonly ElasticConfig? elasticConfig;

    private readonly ElasticClient? elasticClient;

    private readonly List<Type>? entityTypes;

    public EsDbContext(IConfiguration configuration)
    {
        elasticConfig = configuration.GetSection("Elastic").Get<ElasticConfig>();

        if (elasticConfig == null || string.IsNullOrEmpty(elasticConfig.Url))
        {
            Console.WriteLine("Elastic configuration is missing. ElasticSearch integration is disabled.");
            return; // أوقف التهيئة إذا كانت الإعدادات غير موجودة
        }

        var connectionSettings = new ConnectionSettings(new Uri(elasticConfig.Url));

        connectionSettings.DefaultMappingFor<LogRecord>(m => m
            .IndexName($"{elasticConfig!.IndexPrefix}-logs"));

        var assembly = typeof(EsDbContext).Assembly;

        entityTypes =
            (from t in assembly.GetTypes().AsParallel()
             let attributes = t.GetCustomAttributes(typeof(SupportsElasticAttribute), true)
             where attributes != null && attributes.Length > 0
             select t).ToList();

        var migrationsIndexName = ElasticHelper.GetIndexName(elasticConfig!.IndexPrefix, typeof(ElasticMigration));

        connectionSettings.DefaultMappingFor<ElasticMigration>(m => m
            .IndexName(migrationsIndexName));

        foreach (var entityType in entityTypes)
        {
            var indexName = ElasticHelper.GetIndexName(elasticConfig!.IndexPrefix, entityType);

            if (string.IsNullOrEmpty(indexName))
            {
                Console.WriteLine($"Error: Index name is null for entity {entityType.Name}");
            }
            else
            {
                Console.WriteLine($"Index for {entityType.Name}: {indexName}");
            }

            connectionSettings.DefaultMappingFor(entityType, m => m
                .IndexName(indexName));
        }

        elasticClient = new ElasticClient(connectionSettings);

        foreach (var entityType in entityTypes)
        {
            Console.WriteLine($"Entity registered for Elasticsearch: {entityType.Name}");
        }
    }

    public override ElasticClient ElasticClient => elasticClient!;

    public override string IndexPrefix => elasticConfig!.IndexPrefix;

    protected override List<Type> EntityTypes => entityTypes!;
}
