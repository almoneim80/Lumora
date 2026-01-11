namespace Lumora.Plugin.Sms.Configuration;

public class PluginConfig
{
    public string SmsAccessKey { get; set; } = string.Empty;

    public GatewaysConfig SmsGateways { get; set; } = new GatewaysConfig();

    public List<CountryGatewayConfig> SmsCountryGateways { get; set; } = new List<CountryGatewayConfig>();
}

public class GatewaysConfig
{
    public MsegatConfig Msegat { get; set; } = new MsegatConfig();
    public TwilioConfig Twilio { get; set; } = new TwilioConfig();
}

public class CountryGatewayConfig
{
    public string Code { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
}

public class MsegatConfig
{
    public string UserName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = "https://www.msegat.com/gw/sendsms.php";
    public string VerifyOtpUrl { get; set; } = "https://www.msegat.com/gw/verifyOTPCode.php";
    public string SenderId { get; set; } = string.Empty;
}

public class TwilioConfig
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;
}

public class SmsDto
{
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SmsWithTemplateDto
{
    public string Recipient { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public Dictionary<string, string> Placeholders { get; set; } = new();
}
