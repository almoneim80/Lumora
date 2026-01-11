namespace Lumora.Web.Extensions
{
    public static class CorsExtensions
    {
        // Configure CORS
        public static void ConfigureCORS(this WebApplicationBuilder builder)
        {
            var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsConfig>();

            if (corsSettings == null)
            {
                throw new MissingConfigurationException("CORS configuration is mandatory.");
            }

            if (!corsSettings.AllowedOrigins.Any())
            {
                throw new MissingConfigurationException("Specify CORS allowed domains (Use '*' only in development).");
            }

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyMethod().AllowAnyHeader();

                    if (builder.Environment.IsDevelopment())
                    {
                        // Allow all domains in the development environment
                        policy.SetIsOriginAllowed(origin => true);
                    }
                    else
                    {
                        // Allow only authorized domains in the production environment
                        if (corsSettings.AllowedOrigins.Contains("*"))
                        {
                            throw new InvalidOperationException("Using '*' is not allowed in production with AllowCredentials.");
                        }
                        else
                        {
                            policy.WithOrigins(corsSettings.AllowedOrigins.ToArray()).AllowCredentials();
                        }
                    }
                });
            });
        }
    }
}
