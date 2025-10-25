namespace Lumora.Entities.Tables
{
    public class PromoCode : SharedData
    {
        public string Code { get; set; } = default!; // ex: AHMAD20

        public bool IsManual { get; set; } // true if created manually

        public string UserId { get; set; } = default!;
        public virtual User User { get; set; } = default!;

        public int TrainingProgramId { get; set; } = default!;
        public virtual TrainingProgram TrainingProgram { get; set; } = default!;

        public decimal DiscountPercentage { get; set; }

        public decimal CommissionPercentage { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? DeactivatedAt { get; set; }
    }
}
