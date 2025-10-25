namespace Lumora.DTOs.TrainingProgram;

public class ProgramCertificateDetailsDto
{
#nullable disable
    public string Title { get; set; }
    public string StudentName { get; set; }
    public string StudentEmail { get; set; }
    public string StudentPhone { get; set; }
    public string ProgramName { get; set; }
    public DateTimeOffset? IssuedAt { get; set; }
    public string CertificateId { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
}

public class ProgramCertificateListDto
{
    public int CertificateId { get; set; }
    public string CertificateCode { get; set; } = null!;
    public string ProgramName { get; set; } = null!;
    public DateTimeOffset? IssuedAt { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public CertificateStatus Status { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
}

public class ProgramCertificateFileDto
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = "application/pdf";
    public byte[] FileBytes { get; set; } = null!;
}
