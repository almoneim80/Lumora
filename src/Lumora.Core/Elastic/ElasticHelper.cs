namespace Lumora.Elastic;

/// <summary>
/// It has auxiliary functions related to indicators and migrations.
/// </summary>
public class ElasticHelper
{
    /// <summary>
    ///  Creates indicators that have not yet been created.
    /// </summary>
    public static void CreateMissingIndeces(ElasticDbContext dbContext)
    {
        var elasticClient = dbContext.ElasticClient;

        var migrationIndexName = GetIndexName(dbContext.IndexPrefix, typeof(ElasticMigration));

        if (!elasticClient.Indices.Exists(migrationIndexName).Exists)
        {
            elasticClient.Indices.Create(migrationIndexName, f => f
                .Map(mapping => mapping
                    .AutoMap<ElasticMigration>()));
        }
    }

    /// <summary>
    /// Migrates data from an old index to a new one (if changes occur).
    /// </summary>
    public static async Task MigrateIndex(ElasticDbContext dbContext, Type entityType)
    {
        var elasticClient = dbContext.ElasticClient;
        var indexName = GetIndexName(dbContext.IndexPrefix, entityType);

        var oldMigrationIndexName = (await elasticClient.GetIndicesPointingToAliasAsync(indexName)).FirstOrDefault();

        if (string.IsNullOrEmpty(oldMigrationIndexName) && elasticClient.Indices.Exists(indexName).Exists)
        {
            oldMigrationIndexName = indexName;
        }

        var newMigrationIndexName = $"{indexName}-{Guid.NewGuid().ToString()}";

        if (oldMigrationIndexName == null)
        {
            await elasticClient.Indices.CreateAsync(newMigrationIndexName, f => f
                .Map(mapping => mapping
                    .AutoMap(entityType)));

            await elasticClient.Indices.PutAliasAsync(new PutAliasDescriptor(newMigrationIndexName, indexName));
        }
        else
        {
            elasticClient.Indices.Create(newMigrationIndexName, f => f
                .Map(mapping => mapping
                    .AutoMap(entityType)));

            var reindexResponse = await elasticClient.ReindexOnServerAsync(r =>
            {
                r = r.Source(s => s.Index(oldMigrationIndexName));
                r = r.Destination(s => s.Index(newMigrationIndexName));
                r = r.WaitForCompletion();

                return r;
            });

            if (!reindexResponse.IsValid)
            {
                throw reindexResponse.OriginalException;
            }

            elasticClient.Indices.Delete(oldMigrationIndexName);
            elasticClient.Indices.PutAlias(new PutAliasDescriptor(newMigrationIndexName, indexName));
        }
    }

    /// <summary>
    /// Index names are generated based on IndexPrefix and the entity type.
    /// </summary>
    public static string GetIndexName(string indexPrefix, Type entityType)
    {
        if (string.IsNullOrEmpty(indexPrefix))
        {
            throw new ArgumentException("IndexPrefix cannot be null or empty.");
        }

        var tableAttribute = entityType.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;

        if (tableAttribute != null)
        {
            return $"{indexPrefix}-{tableAttribute.Name}";
        }
        else
        {
            return $"{indexPrefix}-{entityType.Name.ToLower()}";
        }
    }
}
