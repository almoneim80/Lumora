using System.Text.Json.Serialization;

namespace Lumora.DTOs;

public class QualificationCreateDto
{
    public string? UserId { get; set; }

    public string? Name { get; set; }
    public DateTimeOffset? DateEarned { get; set; }
    public string? Description { get; set; }
    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class QualificationUpdateDto
{
    public string? Name { get; set; }
    public DateTimeOffset? DateEarned { get; set; }
    public string? Description { get; set; }
    public string? UserId { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class QualificationDetailsDto : QualificationCreateDto
{
    public int Id { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class QualificationExportDto
{
    public string? Name { get; set; }
    public DateTimeOffset? DateEarned { get; set; }
    public string? Description { get; set; }
    public string? UserId { get; set; }
}

public class QualificationImportDto : BaseEntityWithId
{
    public string? UserId { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? DateEarned { get; set; }
    public string? Description { get; set; }
    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
