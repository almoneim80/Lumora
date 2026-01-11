namespace Lumora.Web.Extensions
{
    public static class RequestLimitExtensions
    {
        // Configure import file size
        public static void ConfigureImportSizeLimit(this WebApplicationBuilder builder)
        {
            var maxImportSizeConfig = builder.Configuration.GetValue<string>("ApiSettings:MaxImportSize");

            if (string.IsNullOrEmpty(maxImportSizeConfig))
            {
                throw new MissingConfigurationException("Import file size is mandatory.");
            }

            var maxImportSize = StringHelper.GetSizeInBytesFromString(maxImportSizeConfig);

            if (maxImportSize is null)
            {
                throw new MissingConfigurationException("Max import file size is invalid.");
            }

            builder.WebHost.UseKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = maxImportSize;
            });
        }
    }
}
