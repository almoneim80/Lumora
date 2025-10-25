using Lumora.DTOs.Authentication;
namespace Lumora.DTOs.Club;

public class PostCreateDto
{
    public string? Content { get; set; }

    public string? MediaFile { get; set; }
    public MediaType? MediaType { get; set; }
}

public class PostCreateFormDto
{
    [FromForm]
    public string? Content { get; set; }

    [FromForm]
    public IFormFile? MediaFile { get; set; }

    [FromForm]
    public MediaType? MediaType { get; set; }
}

public class PostStatusUpdateDto
{
    public int PostId { get; set; }
    public ClubPostStatus NewStatus { get; set; }
    public string? RejectionReason { get; set; }
}

public class PostDetailsDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public MediaType MediaType { get; set; }
    public ClubPostStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public PostCreatorData? CreatorInfo { get; set; }
}

public class PostCreatorData
{
#nullable disable
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string City { get; set; }
    public string Sex { get; set; }
    public string Avatar { get; set; }
}
