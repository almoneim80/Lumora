namespace Lumora.Plugin.Sms
{
    public class SmsPlugin : IPlugin
    {
        public static PluginConfig Configuration { get; private set; } = new PluginConfig();
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var pluginConfig = configuration.Get<PluginConfig>();

            if (pluginConfig != null)
            {
                Configuration = pluginConfig;
            }

            // add SMS services
            services.AddTransient<ISmsService, SmsService>();
            services.AddScoped<ITask, SyncSmsLogTask>();
            services.AddScoped<IOtpService, SmsOtpService>();
        }
    }
}
