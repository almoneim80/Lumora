namespace Lumora.Entities.Tables
{
    public class PromoCodeUsage : SharedData
    {
        public int PromoCodeId { get; set; }
        public virtual PromoCode PromoCode { get; set; } = default!;

        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; } = default!;

        public DateTimeOffset UsedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
