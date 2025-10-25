using System.Text.Json.Serialization;

namespace Lumora.DTOs;

public class RefundCreateDto
{
    public int PaymentId { get; set; }
    public string? Reason { get; set; }

    [JsonIgnore]
    public DateTimeOffset? RefundDate { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public ProcessStatus Status { get; set; } = ProcessStatus.Pending;

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class RefundUpdateDto
{
    public int PaymentId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset? RefundDate { get; set; }
    public ProcessStatus? Status { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class RefundDetailsDto : RefundCreateDto
{
    public int Id { get; set; }
    public decimal? Amount { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class RefundExportDto
{
    public int PaymentId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset? RefundDate { get; set; }
    public ProcessStatus Status { get; set; }
}
