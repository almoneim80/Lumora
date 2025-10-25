using Job = Lumora.Entities.Tables.Job;

namespace Lumora.Data
{
    /// <summary>
    /// Jobs DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<Job> Jobs { get; set; } = null!;
        public virtual DbSet<JobApplication> JobApplications { get; set; } = null!;
    }
}
