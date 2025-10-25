namespace Lumora.Entities.Tables;

[Table("sms_log")]
[SupportsElastic]
[SupportsChangeLog]
public class SmsLog : BaseCreateByEntity
{
    [Searchable]
    [Required]
    public string Sender { get; set; } = string.Empty;

    [Searchable]
    [Required]
    public string Recipient { get; set; } = string.Empty;

    [Searchable]
    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    public SmsSendStatus Status { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }

    public static implicit operator SmsLog(List<SmsLog> v)
    {
        throw new NotImplementedException();
    }
}
