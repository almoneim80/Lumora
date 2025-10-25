namespace Lumora.Web.Extensions
{
    public static class AccountExtensions
    {
        // Configure account details
        public static void ConfigureAccountDetails(this WebApplicationBuilder builder)
        {
            var accountDetailsApiConfig = builder.Configuration.GetSection("AccountDetailsApi");

            if (accountDetailsApiConfig == null)
            {
                throw new MissingConfigurationException("Account Details Api configuration is mandatory.");
            }

            builder.Services.Configure<AccountDetailsApiConfig>(accountDetailsApiConfig);
        }
    }
}
