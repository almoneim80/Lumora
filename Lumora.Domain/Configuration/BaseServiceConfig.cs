namespace Lumora.Domain.Configuration
{
    public class BaseServiceConfig
    {
        public string Server { get; set; } = string.Empty;

        public int Port { get; set; } = 0;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
