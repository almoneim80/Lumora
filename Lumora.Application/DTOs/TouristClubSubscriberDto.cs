namespace Lumora.Application.DTOs;

public class TouristClubSubscriberCreateDto
{
    public string UserId { get; set; } = null!;
    public int EventId { get; set; }

    [JsonIgnore]
    public DateTimeOffset? SubscriptionDate { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class TouristClubSubscriberDetailsDto : TouristClubSubscriberCreateDto
{
    public int Id { get; set; }
}

public class TouristClubSubscriberExportDto
{
    public string UserId { get; set; } = null!;
    public int EventId { get; set; }
    public DateTimeOffset? SubscriptionDate { get; set; }
}
