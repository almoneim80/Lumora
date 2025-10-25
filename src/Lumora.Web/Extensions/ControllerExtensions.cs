namespace Lumora.Web.Extensions
{
    public static class ControllerExtensions
    {
        // Configure controllers
        public static void ConfigureControllers(this WebApplicationBuilder builder)
        {
            var controllersBuilder = builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelStateAttribute>();
            })
            .AddJsonOptions(opts =>
            {
                JsonHelper.Configure(opts.JsonSerializerOptions);
            });

            foreach (var plugin in PluginManager.GetPluginList())
            {
                controllersBuilder = controllersBuilder.AddApplicationPart(plugin.GetType().Assembly).AddControllersAsServices();
                plugin.ConfigureServices(builder.Services, builder.Configuration);
                Log.Information($">> Plugin loaded: {plugin.GetType().Assembly.FullName}");
            }
        }

        // Configure conventions
        public static void ConfigureConventions(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            builder.Services.AddControllers(options => options.Conventions.Add(new RouteTokenTransformerConvention(new RouteToKebabCase())));
        }
    }
}
