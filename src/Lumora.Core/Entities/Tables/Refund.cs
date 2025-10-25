namespace Lumora.Entities.Tables
{
    [Table("refunds")]
    [SupportsChangeLog]
    public class Refund : SharedData
    {
        [Required]
        public int PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [Required]
        public RefundStatus Status { get; set; } = RefundStatus.Pending;
    }
}
