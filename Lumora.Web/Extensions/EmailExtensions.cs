namespace Lumora.Web.Extensions
{
    public static class EmailExtensions
    {
        // Configure email verification
        public static void ConfigureEmailVerification(this WebApplicationBuilder builder)
        {
            var emailVerificationConfig = builder.Configuration.GetSection("EmailVerificationApi");

            if (emailVerificationConfig == null)
            {
                throw new MissingConfigurationException("Email Verification Api configuration is mandatory.");
            }

            builder.Services.Configure<EmailVerificationApiConfig>(emailVerificationConfig);
        }

        // Configure email
        public static void ConfigureEmailServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IEmailWithLogService, EmailWithLogService>();
            builder.Services.AddScoped<IEmailFromTemplateService, EmailFromTemplateService>();
        }
    }
}
