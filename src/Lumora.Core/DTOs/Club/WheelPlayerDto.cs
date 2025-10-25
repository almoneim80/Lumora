namespace Lumora.DTOs.Club;

public class WheelPlayerCreateDto
{
    public string PlayerId { get; set; } = null!;
    public DateTimeOffset? PlayedAt { get; set; }
    public int AwardId { get; set; }
    public bool IsFree { get; set; } = true;

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class WheelPlayerUpdateDto
{
    public string? PlayerId { get; set; }
    public DateTimeOffset? PlayedAt { get; set; }
    public int? AwardId { get; set; }
    public bool IsFree { get; set; } = true;
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class WheelPlayerDetailsDto : WheelPlayerCreateDto
{
    public int Id { get; set; }
    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class WheelPlayerExportDto
{
    public string PlayerId { get; set; } = null!;
    public DateTimeOffset? PlayedAt { get; set; }
    public int AwardId { get; set; }
    public bool IsFree { get; set; } = true;
}

public class WheelPlayDto
{
#nullable disable
    public int Id { get; set; }
    public string AwardName { get; set; } = default!;
    public DateTimeOffset? PlayedAt { get; set; }
    public bool IsFree { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public PlayerData PlayerData { get; set; }
}

public class PlayerData
{
#nullable disable
    public string FullName { get; set; }
    public string Email { get; set; }
}

public class UpdateDeliveryStatusDto
{
#nullable disable
    public int PlayId { get; set; }
    public bool IsDelivered { get; set; }
}
