namespace Lumora.Web.Extensions
{
    public static class InfrastructureExtensions
    {
        public static void ConfigureInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Services.AddMemoryCache();
            builder.Configuration.AddUserSecrets(typeof(Program).Assembly);
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddDataProtection();
            builder.Services.AddLogging();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfiles());
                cfg.AllowNullCollections = true;
            });

            builder.Services.AddAutoMapper(typeof(TrainingProgramProfile));

            builder.Services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetRequiredService<IActionContextAccessor>().ActionContext!;
                return new UrlHelper(actionContext);
            });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
            });
        }
    }
}
