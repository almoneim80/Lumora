using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lumora.Web.Interfaces
{
    public interface ISwaggerConfigurator
    {
        void ConfigureSwagger(SwaggerGenOptions options, OpenApiInfo settings);
    }
}
