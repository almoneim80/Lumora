namespace Lumora.Infrastructure.Data
{
    /// <summary>
    /// Jobs DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<Domain.Entities.Tables.Job> Jobs { get; set; } = null!;
        public virtual DbSet<JobApplication> JobApplications { get; set; } = null!;
    }
}
