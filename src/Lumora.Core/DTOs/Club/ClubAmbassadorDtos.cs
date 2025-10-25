using Lumora.DTOs.Authentication;

namespace Lumora.DTOs.Club;

public class AmbassadorAssignDto
{
    public string UserId { get; set; } = null!;
    public int DurationInDays { get; set; }
    public string? Reason { get; set; }
    public int? ClubPostId { get; set; }
}

public class AmbassadorDetailsDto
{
    public int Id { get; set; }
    public DateTimeOffset AppointedStartDate { get; set; }
    public DateTimeOffset AppointedEndDate { get; set; }
    public string? AppointedReason { get; set; }
    public AmbassadorPost? AmbassadorPost { get; set; }
    public AmbassadorData? CreatorInfo { get; set; }
}

public class AmbassadorData
{
#nullable disable
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string City { get; set; }
    public string Sex { get; set; }
    public string Avatar { get; set; }
}

public class AmbassadorPost
{
#nullable enable
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public MediaType MediaType { get; set; }
    public ClubPostStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}
