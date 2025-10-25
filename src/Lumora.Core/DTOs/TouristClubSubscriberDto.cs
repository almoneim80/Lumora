using System.Text.Json.Serialization;

namespace Lumora.DTOs;

public class TouristClubSubscriberCreateDto
{
    public string UserId { get; set; } = null!;
    public int EventId { get; set; }

    [JsonIgnore]
    public DateTimeOffset? SubscriptionDate { get; set; } = DateTimeOffset.UtcNow.DateTime;

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow.DateTime;
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
