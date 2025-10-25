namespace Lumora.Entities.Tables
{
    [Table("payment_items")]
    [SupportsChangeLog]
    public class PaymentItem : SharedData
    {
        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; } = null!;

        public PaymentItemType ItemType { get; set; }
        public int ItemId { get; set; }
        public decimal Amount { get; set; }
    }
}
