namespace Lumora.Application.Configuration
{
    public class FileUploadSettings
    {
        public FileCategorySetting? Images { get; set; }
        public FileCategorySetting? Videos { get; set; }
        public FileCategorySetting? Documents { get; set; }
    }
}
