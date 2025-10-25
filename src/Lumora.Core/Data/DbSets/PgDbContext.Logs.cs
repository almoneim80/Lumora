namespace Lumora.Data
{
    /// <summary>
    /// Email DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<TaskExecutionLog> TaskExecutionLogs { get; set; } = null!;
        public virtual DbSet<SmsLog> SmsLogs { get; set; } = null!;
        public virtual DbSet<EmailLog> EmailLogs { get; set; } = null!;
        public virtual DbSet<ChangeLogTaskLog> ChangeLogTaskLogs { get; set; } = null!;
        public virtual DbSet<ChangeLog> ChangeLogs { get; set; } = null!;
    }
}
