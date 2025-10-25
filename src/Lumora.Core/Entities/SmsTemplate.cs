namespace Lumora.Entities;

[Table("sms_templates")]
public class SmsTemplate : SharedData
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
}
