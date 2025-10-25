namespace Lumora.Data
{
    /// <summary>
    /// Email DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<EmailGroup> EmailGroups { get; set; } = null!;
        public virtual DbSet<EmailSchedule> EmailSchedules { get; set; } = null!;
        public virtual DbSet<EmailTemplate> EmailTemplates { get; set; } = null!;
        public virtual DbSet<ContactEmailSchedule> ContactEmailSchedules { get; set; } = null!;
    }
}
