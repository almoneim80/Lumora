namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class ProgramEnrollment : SharedData
    {
        public int ProgramId { get; set; }
        public virtual TrainingProgram TrainingProgram { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        //public string PaymentId { get; set; } = null!;
        //public virtual Payment Payment { get; set; } = null!;

        public DateTimeOffset? EnrolledAt { get; set; }
        public EnrollmentStatus EnrollmentStatus { get; set; }

        public virtual ProgramCertificate? Certificate { get; set; }
    }
}
