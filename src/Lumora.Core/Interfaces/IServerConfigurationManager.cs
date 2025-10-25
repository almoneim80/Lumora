namespace Lumora.Interfaces;
public interface IServerConfigurationManager
{
    ServerConfiguration Configuration { get; }
}

public class ServerConfiguration : IServerConfigurationManager
{
    public string MetadataCountryCode { get; set; } = "SA";
    public string UICulture { get; set; } = "ar";
    public ServerConfiguration Configuration => throw new NotImplementedException();
}
