namespace Lumora.Domain.Entities;
[Table("contact")]
[SupportsElastic]
[SupportsChangeLog]
public class Contact : SharedData
{
    private string email = string.Empty;

    [Searchable]
    public string? Prefix { get; set; }

    [Searchable]
    public string? FirstName { get; set; }

    [Searchable]
    public string? MiddleName { get; set; }

    [Searchable]
    public string? LastName { get; set; }

    [Searchable]
    public DateTimeOffset? Birthday { get; set; }

    [Required]
    [Searchable]
    public string Email
    {
        get
        {
            return email;
        }

        set
        {
            email = value.ToLower();
        }
    }

    [Searchable]
    public Continent? ContinentCode { get; set; }

    [Searchable]
    public Country? CountryCode { get; set; }

    [Searchable]
    public string? CityName { get; set; }

    [Searchable]
    public string? Address1 { get; set; }

    [Searchable]
    public string? Address2 { get; set; }

    [Searchable]
    public string? JobTitle { get; set; }

    [Searchable]
    public string? CompanyName { get; set; }

    [Searchable]
    public string? Department { get; set; }

    [Searchable]
    public string? State { get; set; }

    [Searchable]
    public string? Zip { get; set; }

    [Searchable]
    public string? Phone { get; set; }

    public int? Timezone { get; set; }

    [Searchable]
    public string? Language { get; set; }

    [Searchable]
    public Dictionary<string, string>? SocialMedia { get; set; }

    [Required]
    public int DomainId { get; set; }

    public virtual WebDomain? Domain { get; set; }

    public int? AccountId { get; set; }

    public int? UnsubscribeId { get; set; }

    public virtual Unsubscribe? Unsubscribe { get; set; }
    public virtual ICollection<ContactEmailSchedule>? ContactEmailSchedules { get; set; }

    /*
    public static string GetCommentableType()
    {
        return "Contact";
    }
    */
}
