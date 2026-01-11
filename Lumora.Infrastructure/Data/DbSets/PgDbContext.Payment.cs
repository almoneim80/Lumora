namespace Lumora.Infrastructure.Data
{
    /// <summary>
    /// Payments DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<Refund> Refunds { get; set; } = null!;
        public virtual DbSet<PaymentItem> PaymentItems { get; set; } = null!;
    }
}
