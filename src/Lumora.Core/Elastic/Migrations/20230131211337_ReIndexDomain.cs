using Lumora.Elastic;

namespace Lumora.Elastic.Migrations;

[ElasticMigration("20230131211337_ReIndexDomain")]
public class ReIndexDomain : ElasticMigration
{
    public override async Task Up(ElasticDbContext context)
    {
        await ElasticHelper.MigrateIndex(context, typeof(Domain));
    }
}
