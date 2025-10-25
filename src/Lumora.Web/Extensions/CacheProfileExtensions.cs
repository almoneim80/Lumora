namespace Lumora.Web.Extensions
{
    public static class CacheProfileExtensions
    {
        // Configure cache
        public static void ConfigureCacheProfiles(this WebApplicationBuilder builder)
        {
            var cacheProfiles = builder.Configuration.GetSection("CacheProfiles").Get<List<CacheProfileSettings>>();

            if (cacheProfiles == null)
            {
                throw new MissingConfigurationException("Image Wejhaload configuration is mandatory.");
            }

            builder.Services.AddControllers(options =>
            {
                foreach (var item in cacheProfiles)
                {
                    options.CacheProfiles.Add(
                        item!.Type!,
                        new CacheProfile()
                        {
                            Duration = item!.DurationInDays!,
                            VaryByHeader = item!.VaryByHeader!,
                        });
                }
            });
        }
    }
}
