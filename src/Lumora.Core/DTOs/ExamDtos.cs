using System.Text.Json.Serialization;

namespace Lumora.DTOs;

public class ExamCreateDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public decimal? MinMark { get; set; }
    public decimal? MaxMark { get; set; }
    public int? DurationInMinutes { get; set; }
    public ExamParentEntityType? ParentEntityType { get; set; }

    public int? CourseId { get; set; }
    public int? LessonId { get; set; }
    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class ExamUpdateDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public decimal? MinMark { get; set; }
    public decimal? MaxMark { get; set; }
    public int? DurationInMinutes { get; set; }
    public ExamParentEntityType? ParentEntityType { get; set; }
    public int CourseId { get; set; }
    public int LessonId { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class ExamDetailsDto : ExamCreateDto
{
    public int Id { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ExamExportDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public decimal? MinMark { get; set; }
    public decimal? MaxMark { get; set; }
    public int? DurationInMinutes { get; set; }

    public int? CourseId { get; set; }
    public int? LessonId { get; set; }
}

public class ExamImportDto : BaseEntityWithId
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public decimal? MinMark { get; set; }
    public decimal? MaxMark { get; set; }
    public int? DurationInMinutes { get; set; }
    public ExamParentEntityType? ParentEntityType { get; set; }

    public int? CourseId { get; set; }
    public int? LessonId { get; set; }

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}
