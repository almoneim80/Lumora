namespace Lumora.Application.Configuration
{
    public class ApiSettingsConfig
    {
        public int MaxListSize { get; set; }

        public string MaxImportSize { get; set; } = string.Empty;

        public string DefaultLanguage { get; set; } = "en-US";
    }
}
