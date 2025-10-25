using System.Text.Json.Serialization;

namespace Lumora.DTOs;

public class TouristClubCreateDto
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class TouristClubUpdateDto
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class TouristClubDetailsDto : TouristClubCreateDto
{
    public int Id { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class TouristClubExportDto
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

public class TouristClubImportDto : BaseEntityWithId
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? StartDate { get; set; } 
    public DateTimeOffset? EndDate { get; set; }
    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}
