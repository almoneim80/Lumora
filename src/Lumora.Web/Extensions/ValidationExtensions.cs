namespace Lumora.Web.Extensions
{
    public static class ValidationExtensions
    {
        public static void ConfigureValidation(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<Program>();
        }
    }
}
