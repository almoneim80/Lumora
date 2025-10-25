namespace Lumora.Web.Extensions
{
    public static class ApiSettingsExtensions
    {
        // Configure api settings
        public static void ConfigureApiSettings(this WebApplicationBuilder builder)
        {
            var apiSettingsConfig = builder.Configuration.GetSection("ApiSettings");

            if (apiSettingsConfig == null)
            {
                throw new MissingConfigurationException("Api settings configuration is mandatory.");
            }

            builder.Services.Configure<ApiSettingsConfig>(apiSettingsConfig);
        }
    }
}
