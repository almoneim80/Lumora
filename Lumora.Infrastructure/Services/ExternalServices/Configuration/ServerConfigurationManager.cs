namespace Lumora.Services;

public class ServerConfigurationManager : IServerConfigurationManager
{
    public ServerConfiguration Configuration { get; } = new ServerConfiguration();
}
