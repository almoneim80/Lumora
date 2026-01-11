namespace Lumora.Web.Extensions
{
    public static class SwaggerExtensions
    {
        // Configure swagger
        public static void ConfigureSwagger(this WebApplicationBuilder builder)
        {
            var openApiInfo = new OpenApiInfo()
            {
                Version = typeof(Program).Assembly.GetName().Version!.ToString() ?? "1.0.0",
                Title = "Lumora API",
                Description = "Lumora Backend API",
            };
            var swaggerConfigurators = from p in PluginManager.GetPluginList()
                                       where p is ISwaggerConfigurator
                                       select p as ISwaggerConfigurator;

            builder.Services.AddSwaggerGen(config =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);

                foreach (var swaggerConfigurator in swaggerConfigurators)
                {
                    swaggerConfigurator.ConfigureSwagger(config, openApiInfo);
                }

                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "Copy 'Bearer ' + valid JWT token into field",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                });

                config.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                },
            });

                config.EnableAnnotations();

                config.SupportNonNullableReferenceTypes();

                config.SchemaFilter<CustomSwaggerScheme>();

                config.UseInlineDefinitionsForEnums();

                config.SwaggerDoc("v1", openApiInfo);

                var conf = builder.Configuration.GetSection("Entities").Get<EntitiesConfig>();
                config.DocumentFilter<SwaggerEntitiesFilter>(conf);
            });
        }
    }
}
