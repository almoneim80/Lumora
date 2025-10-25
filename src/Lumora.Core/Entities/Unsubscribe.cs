using System.Text.Json.Serialization;

namespace Lumora.Entities;

[Table("unsubscribe")]
[SupportsChangeLog]
[SupportsElastic]
public class Unsubscribe : SharedData, IUnsubscribe
{
    [Searchable]
    public string Reason { get; set; } = string.Empty;

    public int? ContactId { get; set; }

    [JsonIgnore]
    public virtual Contact? Contact { get; set; }
    public string? CreatedByIp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string? CreatedById { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string? CreatedByUserAgent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

public interface IUnsubscribe
{
    [Required]
    public DateTimeOffset? CreatedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public string? CreatedById { get; set; }

    public string? CreatedByUserAgent { get; set; }
}
