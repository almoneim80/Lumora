using FluentValidation;

namespace Lumora.Core.Services
{
    public static class ValidationRegistration
    {
        public static IServiceCollection AddCoreValidators(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
