using ILogger = Microsoft.Extensions.Logging.ILogger;
namespace Lumora.Infrastructure.StaticContentInfra
{
    public static class StaticContentSeeder
    {
        public static async Task SeedAsync(PgDbContext dbContext, ILogger logger)
        {
            var existingKeys = await dbContext.StaticContents
                .Select(c => new { c.Key, c.Language })
                .ToListAsync();

            int addedCount = 0;

            foreach (var pair in StaticContentDefaults.GetAll("ar"))
            {
                if (!existingKeys.Exists(e => e.Key == pair.Key && e.Language == "ar"))
                {
                    dbContext.StaticContents.Add(new StaticContent
                    {
                        Key = pair.Key,
                        Value = pair.Value,
                        Language = "ar",
                        IsActive = true,
                        LastModified = DateTimeOffset.UtcNow
                    });
                    addedCount++;
                }
            }

            foreach (var pair in StaticContentDefaults.GetAll("en"))
            {
                if (!existingKeys.Exists(e => e.Key == pair.Key && e.Language == "en"))
                {
                    dbContext.StaticContents.Add(new StaticContent
                    {
                        Key = pair.Key,
                        Value = pair.Value,
                        Language = "en",
                        IsActive = true,
                        LastModified = DateTimeOffset.UtcNow
                    });
                    addedCount++;
                }
            }

            await dbContext.SaveChangesAsync();

            if (addedCount > 0)
                logger.LogInformation("StaticContentSeeder - Seeded {Count} new content entries.", addedCount);
            else
                logger.LogInformation("StaticContentSeeder - No new content was added.");
        }
    }
}
