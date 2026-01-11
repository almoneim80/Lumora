namespace Lumora.Domain.Entities;

[Table("unsubscribe")]
[SupportsChangeLog]
[SupportsElastic]
public class Unsubscribe : SharedData
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
