using System.Text.Json.Serialization;

namespace Lumora.DTOs.Email;

public class EmailVerifyDetailsDto : DomainDetailsDto
{
    public string EmailAddress { get; set; } = string.Empty;
}

public class EmailVerifyInfoDto
{
    public string EmailAddress { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonHelper.BooleanConverter))]
    public bool FreeCheck { get; set; }

    [JsonConverter(typeof(JsonHelper.BooleanConverter))]
    public bool DisposableCheck { get; set; }

    [JsonConverter(typeof(JsonHelper.BooleanConverter))]
    public bool CatchAllCheck { get; set; }
}

// DTO لطلب إرسال البريد الإلكتروني
public class SendEmailRequest
{
    public string Subject { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string[] Recipients { get; set; } = Array.Empty<string>();
    public string Body { get; set; } = string.Empty;
    public List<AttachmentDto>? Attachments { get; set; }
}

