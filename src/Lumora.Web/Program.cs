using Microsoft.AspNetCore.Http.Features;
using Lumora.Web.Extensions;
using Xabe.FFmpeg;

namespace Lumora
{
    public class Program
    {
        private static readonly List<string> AppSettingsFiles = new();
        private static WebApplication? app;

        public static WebApplication? GetApp() => app;
        public static void AddAppSettingsJsonFile(string path) => AppSettingsFiles.Add(path);

        public static async Task Main(string[] args)
        {
            // Excel configuration
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var builder = WebApplication.CreateBuilder(args);

            // Load extra appsettings files
            AppSettingsFiles.ForEach(path =>
            {
                builder.Configuration.AddJsonFile(path, optional: false, reloadOnChange: true);
                builder.Configuration.AddJsonFile("pluginsettings.json", optional: false, reloadOnChange: true);
                Log.Information("AppSettingsFile", path + " loaded.");
            });

            // Configure ffmpeg
            var ffmpegSettings = builder.Configuration.GetSection("FfmpegSettings").Get<FfmpegSettings>();
            if (!string.IsNullOrWhiteSpace(ffmpegSettings?.ExecutablesPath))
            {
                FFmpeg.SetExecutablesPath(ffmpegSettings.ExecutablesPath);
            }

            // Configure logs
            Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
            builder.ConfigureLogs();

            // Initialize plugin manager
            PluginManager.Init(builder.Configuration);

            // Configure database
            var postgres = builder.Configuration.GetSection("Postgres");
            var connectionString = $"Host={postgres["Server"]};Port={postgres["Port"]};Database={postgres["Database"]};Username={postgres["UserName"]};Password={postgres["Password"]}";
            builder.Services.AddDbContext<PgDbContext>(options =>
                options.UseNpgsql(connectionString).UseLazyLoadingProxies());

            builder.Services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<DefaultRolesConfig>>().Value);

            // Configure identity and authentication
            IdentityHelper.ConfigureAuthentication(builder);

            // Register services
            builder.Services.RegisterAppServices(builder.Configuration);

            // Configure global infrastructure
            builder.ConfigureInfrastructure();

            // Configure validation
            builder.Services.ConfigureValidation();

            // Configure application-specific services
            builder.ConfigureApiSettings();
            builder.ConfigureAccountDetails();
            builder.ConfigureEmailVerification();
            builder.ConfigureImageWejhaload();
            builder.ConfigureFileWejhaload();
            builder.ConfigureIpDetailsResolver();
            builder.ConfigureTasks();
            builder.ConfigureEmailServices();
            builder.ConfigureImportSizeLimit();

            // Configure application behavior
            builder.ConfigureCacheProfiles();
            builder.ConfigureControllers();
            builder.ConfigureConventions();

            // Configure integrations
            builder.ConfigureSwagger();
            builder.ConfigureQuartz();
            builder.ConfigureCORS();
            builder.ConfigureIdentity();

            // Set multipart upload limit
            var maxSizeString = builder.Configuration.GetSection("ApiSettings:MaxImportSize").Value;

            if (string.IsNullOrWhiteSpace(maxSizeString))
            {
                throw new Exception("MaxImportSize configuration is missing or empty.");
            }

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = StringHelper.GetSizeInBytesFromString(maxSizeString) ?? 1024L * 1024 * 1024;
            });

            // Configure controller-specific options
            builder.Services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
                options.OutputFormatters.RemoveType<StringOutputFormatter>();
                options.InputFormatters.Add(new CsvInputFormatter());
                options.OutputFormatters.Add(new CsvOutputFormatter());
                options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Build app
            app = builder.Build();
            PluginManager.Init(app);

            // Middleware pipeline
            app.Services.GetRequiredService<ILocalizationManager>();
            app.UseHttpsRedirection();
            app.UseForwardedHeaders();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCors();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<CultureMiddleware>();
            app.UseMiddleware<PermissionMiddleware>();
            app.Use(async (context, next) => await next());

            if (builder.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/error");

            app.UseCookiePolicy();
            app.MapControllers();


            // Run app
            await app.ApplyMigrationsAndSeedAsync();

            app.Run();

            // Migrate ES context
            using var scope = app.Services.CreateScope();
            var esDbContext = scope.ServiceProvider.GetRequiredService<EsDbContext>();
            esDbContext.Migrate();
        }
    }
}
