namespace Lumora.Domain.Entities;

[Table("domain")]
[SupportsChangeLog]
[SupportsElastic]

public class WebDomain : SharedData
{
    private string name = string.Empty;

    [Required]
    [Searchable]
    public string Name
    {
        get
        {
            return name;
        }

        set
        {
            name = value.ToLower();
        }
    }

    [Searchable]
    public string? Title { get; set; }

    [Searchable]
    public string? Description { get; set; }

    public string? Url { get; set; }

    public string? FaviconUrl { get; set; }

    public bool? HttpCheck { get; set; }

    public bool? Free { get; set; }

    public bool? Disposable { get; set; }

    public bool? CatchAll { get; set; }

    [Nested]
    public virtual List<DnsRecord>? DnsRecords { get; set; }

    public bool? DnsCheck { get; set; }

    public bool? MxCheck { get; set; }

    public int? AccountId { get; set; }

    public AccountSyncStatus AccountStatus { get; set; } = AccountSyncStatus.NotIntended;
}
