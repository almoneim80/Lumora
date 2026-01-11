namespace Lumora.Infrastructure.Interfaces
{
    public interface IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}
