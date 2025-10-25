namespace Lumora.Web.Extensions
{
    public static class IpDetailsExtensions
    {
        // Configure Ip Details Resolver
        public static void ConfigureIpDetailsResolver(this WebApplicationBuilder builder)
        {
            var geolocationApiConfig = builder.Configuration.GetSection("GeolocationApi");

            if (geolocationApiConfig == null)
            {
                throw new MissingConfigurationException("Geo Location Api configuration is mandatory.");
            }

            builder.Services.Configure<GeolocationApiConfig>(geolocationApiConfig);
        }
    }
}
