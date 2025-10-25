using System.Text.Json.Serialization;

namespace Lumora.DTOs;

// Create DTO
public class ProgressCreateDto
{
    public string UserId { get; set; } = null!;
    public int? CourseId { get; set; }
    public int? LessonId { get; set; }
    public int? PathId { get; set; }
    public decimal ProgressPercentage { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public DateTimeOffset? CompletedAt { get; set; }
}

// Details DTO
public class ProgressDetailsDto : ProgressCreateDto
{
    public int Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
