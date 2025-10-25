namespace Lumora.DTOs.AffiliateMarketing;

public class PromoCodeCreateDto
{
    public int TrainingProgramId { get; set; }

    public string? Code { get; set; }

    public decimal DiscountPercentage { get; set; }

    public decimal CommissionPercentage { get; set; }
}

public class PromoCodeReportDto
{
    public string Code { get; set; } = default!;
    public string UserFullName { get; set; } = default!;
    public string ProgramTitle { get; set; } = default!;
    public bool IsActive { get; set; }
    public int UsageCount { get; set; }
}
