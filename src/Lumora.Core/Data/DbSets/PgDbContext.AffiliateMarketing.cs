namespace Lumora.Data
{
    /// <summary>
    /// Affiliate Marketing DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<PromoCodeUsage> PromoCodeUsages { get; set; } = null!;
        public virtual DbSet<PromoCode> PromoCodes { get; set; } = null!;
    }
}
