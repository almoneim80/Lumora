using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lumora.Interfaces;

public interface ISwaggerConfigurator
{
    void ConfigureSwagger(SwaggerGenOptions options, OpenApiInfo settings);
}
