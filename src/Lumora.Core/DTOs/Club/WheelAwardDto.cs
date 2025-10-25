namespace Lumora.DTOs.Club;

public class WheelAwardCreateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Probability { get; set; }
    public AwardType Type { get; set; }

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class WheelAwardUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Probability { get; set; }
    public AwardType? Type { get; set; }

    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}

public class WheelAwardDetailsDto : WheelAwardCreateDto
{
    public int Id { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class WheelAwardExportDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Probability { get; set; }
}

public class WheelAwardImportDto : BaseEntityWithId
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Probability { get; set; }
    public AwardType? Type { get; set; }

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
}
