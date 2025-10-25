#nullable disable

namespace Lumora.DTOs;

public class OtpVia
{
    public OtpVia(string plain, string url = null)
    {
        Plain = plain;
        Url = url;
    }

    public string Plain { get; set; } // (OTP Code)
    public string Url { get; set; } // Verification link (optional)
}

public class IdPlain
{
    public IdPlain(string id, string plain)
    {
        Id = id;
        Plain = plain;
    }

    public string Id { get; set; }
    public string Plain { get; set; } // (OTP)
}
