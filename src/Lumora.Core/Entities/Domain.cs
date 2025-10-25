namespace Lumora.Entities;

public enum AccountSyncStatus
{
    NotIntended = 0,
    NotInitialized = 1,
    Successful = 2,
    Failed = 3,
}

[Table("domain")]
[SupportsChangeLog]
[SupportsElastic]
[Index(nameof(Name), IsUnique = true)]

public class Domain : SharedData
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
    [Column(TypeName = "jsonb")]
    public List<DnsRecord>? DnsRecords { get; set; }

    public bool? DnsCheck { get; set; }

    public bool? MxCheck { get; set; }

    public int? AccountId { get; set; }

    public AccountSyncStatus AccountStatus { get; set; } = AccountSyncStatus.NotIntended;
}

public class DnsRecord
{
    public string DomainName { get; set; } = string.Empty;

    public string RecordClass { get; set; } = string.Empty;

    public string RecordType { get; set; } = string.Empty;

    public int TimeToLive { get; set; }

    public string Value { get; set; } = string.Empty;
}
