namespace Lumora.Web.Extensions
{
    public static class MediaExtensions
    {
        // Configure image Wejhaload
        public static void ConfigureImageWejhaload(this WebApplicationBuilder builder)
        {
            var imageWejhaloadConfig = builder.Configuration.GetSection("Media");

            if (imageWejhaloadConfig == null)
            {
                throw new MissingConfigurationException("Image Wejhaload configuration is mandatory.");
            }

            builder.Services.Configure<MediaConfig>(imageWejhaloadConfig);
        }

        // Configure file Wejhaload
        public static void ConfigureFileWejhaload(this WebApplicationBuilder builder)
        {
            var fileWejhaloadConfig = builder.Configuration.GetSection("File");

            if (fileWejhaloadConfig == null)
            {
                throw new MissingConfigurationException("File Wejhaload configuration is mandatory.");
            }

            builder.Services.Configure<FileConfig>(fileWejhaloadConfig);
        }
    }
}
