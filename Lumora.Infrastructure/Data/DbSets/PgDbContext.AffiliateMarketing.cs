namespace Lumora.Infrastructure.Data
{
    /// <summary>
    /// Promo DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<PromoCodeUsage> PromoCodeUsages { get; set; } = null!;
        public virtual DbSet<PromoCode> PromoCodes { get; set; } = null!;
    }
}
