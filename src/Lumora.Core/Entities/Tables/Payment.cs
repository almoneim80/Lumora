namespace Lumora.Entities.Tables
{
    [Table("payments")]
    [SupportsChangeLog]
    public class Payment : SharedData
    {
        [Required]
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "SAR";

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public PaymentPurpose PaymentPurpose { get; set; }

        [Required]
        public PaymentGatewayType PaymentGateway { get; set; }

        [MaxLength(100)]
        public string? GatewayReferenceId { get; set; }

        public DateTimeOffset? PaidAt { get; set; }

        public string? Metadata { get; set; } // Can be JSON string

        public int? PromoCodeId { get; set; }
        public virtual PromoCode? PromoCode { get; set; }

        // Navigation property
        public virtual ICollection<PaymentItem> Items { get; set; } = new List<PaymentItem>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
