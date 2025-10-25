using System.Text.Json.Serialization;

namespace Lumora.DTOs;

public class ExamResultCreateDto
{
    public decimal? Mark { get; set; }
    public int ExamId { get; set; }
    public string UserId { get; set; } = null!;
    public string? Status { get; set; }
    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class ExamResultUpdateDto
{
    public decimal? Mark { get; set; }
    public int ExamId { get; set; }
    public string UserId { get; set; } = null!;
    public string? Status { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class ExamResultDetailsDto : ExamResultCreateDto
{
    public int Id { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ExamResultExportDto
{
    public decimal? Mark { get; set; }
    public int? ExamId { get; set; }
    public string? UserId { get; set; }
    public string? Status { get; set; }
}
