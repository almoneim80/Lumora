namespace Lumora.Elastic;

/// <summary>
/// Manages migrations within Elasticsearch.
/// </summary>
[Table("migration")] // Data on migrations is stored in an index called “migration”.
public class ElasticMigration
{
    public ElasticMigration()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

        ProductVersion = fileVersionInfo.ProductVersion!;

        if (GetType().GetCustomAttributes(typeof(ElasticMigrationAttribute), true).FirstOrDefault() is ElasticMigrationAttribute elasticMigrationAttribute)
        {
            MigrationId = elasticMigrationAttribute.Id;
        }
    }

    // Each Migration is identified by a MigrationId.
    public string MigrationId { get; set; } = string.Empty;

    public string ProductVersion { get; set; }

    /// <summary>
    /// Customized to execute the migration.
    /// </summary>
    public virtual async Task Up(ElasticDbContext context)
    {
        await Task.CompletedTask;

        throw new NotImplementedException();
    }
}

public class ElasticMigrationAttribute : Attribute
{
    public ElasticMigrationAttribute(string id)
    {
        Id = id;
    }

    public string Id { get; set; }
}
