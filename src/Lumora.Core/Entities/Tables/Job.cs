#nullable disable
namespace Lumora.Entities.Tables
{
    [Table("Jobs")]
    [SupportsChangeLog]
    public class Job : SharedData
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public JobType JobType { get; set; }
        public decimal Salary { get; set; }
        public string Employer { get; set; } = null!;
        public string EmployerInfo { get; set; } = null!;
        public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public WorkplaceCategory WorkplaceCategory { get; set; }
    }
}
