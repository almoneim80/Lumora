using Serilog;

namespace Lumora.Elastic;

public abstract class ElasticDbContext
{
    // The main communication interface with Elasticsearch.
    public abstract ElasticClient ElasticClient { get; }

    // The starting clip for the index names.
    public abstract string IndexPrefix { get; }

    // A list of the types of entities being dealt with.
    protected abstract List<Type> EntityTypes { get; }

    /// <summary>
    /// This is where you select the types that need to be “Migrated”.
    /// ElasticHelper is used to create the missing pointers.
    /// Ensures that the required updates (Migrations) are performed if they have not already been performed.
    /// </summary>
    public virtual void Migrate()
    {
        Log.Information("Starting Elasticsearch migration...");

        ElasticHelper.CreateMissingIndeces(this);

        var allMigrationsTypes = Assembly.GetAssembly(typeof(ElasticMigration))!.GetTypes()
                                    .Where(
                                        myType => myType.IsClass
                                        && !myType.IsAbstract
                                        && myType.IsSubclassOf(typeof(ElasticMigration)))
                                    .OrderBy(type => type.Name);

        var pastMigrationIds = ElasticClient.Search<ElasticMigration>(s => s.Size(10000)).Documents // 10000 is max possible amount of migrations
                                .Select(m => m.MigrationId).ToList();

        foreach (var type in allMigrationsTypes)
        {
            var migration = (ElasticMigration)Activator.CreateInstance(type)!;

            if (!pastMigrationIds.Contains(migration.MigrationId))
            {
                migration.Up(this).Wait();
                ElasticClient.Index<ElasticMigration>(migration, s => s);
            }
        }

        Log.Information("Elasticsearch indices migration completed successfully.");
    }
}
