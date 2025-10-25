namespace Lumora.Entities.Tables;
[SupportsElastic]
[SupportsChangeLog]
[Table("users")]
public class User : IdentityUser, ISharedData
{
    [Searchable]
    public string? FullName { get; set; }
    public string? City { get; set; }
    public string? Sex { get; set; }
    public string? AboutMe { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Avatar { get; set; }

    // additional properties
    public DateTimeOffset? LastTimeLoggedIn { get; set; }
    public bool IsActive { get; set; }
    public string? DeActiveReason { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset? SoftDeleteExpiration { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? AdditionalData { get; set; }

    // navigational properties
    public virtual ICollection<ProgramEnrollment>? ProgramEnrollments { get; set; }
    public virtual ICollection<TraineeProgress>? StudentProgresses { get; set; }
    public virtual ICollection<JobApplication>? JobApplications { get; set; }
    public virtual ICollection<Payment>? Payments { get; set; }
    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new HashSet<TestAttempt>();
}
