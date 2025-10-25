namespace Lumora.Plugin.Sms.DTOs;

public class CreateSmsDto
{
    public string? Sender { get; set; }
    public string? Recipient { get; set; }
    public string? Message { get; set; }
    public SmsSendStatus Status { get; set; } = SmsSendStatus.NotSent;
    public string Source { get; set; } = "Lumora";
    public string? SourceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}

public class SmsDetailsDto
{
    [Required]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Message { get; set; } = string.Empty;
}

public class AddSmsResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public int? SmsId { get; set; }
}
