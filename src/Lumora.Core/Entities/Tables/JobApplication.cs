namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class JobApplication : SharedData
    {
        public int JobId { get; set; }
        public virtual Job Job { get; set; } = null!;
        public string ApplicantUserId { get; set; } = null!;
        public virtual User ApplicantUser { get; set; } = null!;
        public string? CoverLetter { get; set; }
        public string? ResumeUrl { get; set; }
        public JobApplicationStatus Status { get; set; } 
        public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
