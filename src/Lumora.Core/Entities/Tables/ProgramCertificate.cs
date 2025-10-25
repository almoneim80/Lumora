namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class ProgramCertificate : SharedData
    {
        public int EnrollmentId { get; set; }
        public virtual ProgramEnrollment ProgramEnrollment { get; set; } = null!;

        public string CertificateId { get; set; } = null!;
        public DateTimeOffset? IssuedAt { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }

        public DeliveryMethod DeliveryMethod { get; set; }
        public string? ShippingStatus { get; set; }
        public string? ShippingAddress { get; set; }

        public CertificateStatus Status { get; set; } = CertificateStatus.Pending;
        public string? IssuedBy { get; set; }
        public string? Notes { get; set; }
        public string? VerificationCode { get; set; }
    }
}
