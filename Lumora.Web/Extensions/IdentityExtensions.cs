namespace Lumora.Web.Extensions
{
    public static class IdentityExtensions
    {
        // Configure identity
        public static void ConfigureIdentity(this WebApplicationBuilder builder)
        {
            var jwtConfig = builder.Configuration.GetSection("Jwt");

            if (jwtConfig == null)
            {
                throw new MissingConfigurationException("Jwt configuration is mandatory.");
            }

            builder.Services.Configure<JwtConfig>(jwtConfig);
        }
    }
}
