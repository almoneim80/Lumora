namespace Lumora.DTOs.Programs;

public class AttachmentDetailsDto
{
    public int AttachmentId { get; set; }
    public string AttachmentFileUrl { get; set; } = string.Empty;
    public int AttachmentOpenCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class LessonAttachmentDetailsDto
{
#nullable disable
    public int AttachmentId { get; set; }
    public int LessonId { get; set; }
    public string LessonName { get; set; }
    public string AttachmentUrl { get; set; }
    public int OpenCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class LessonAttachmentUpdateDto
{
#nullable enable
    public int? LessonId { get; set; }
    public string? FileUrl { get; set; }

    [JsonIgnore]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class LessonAttachmentUpdateFormDto
{
#nullable enable
    [FromForm]
    public int? LessonId { get; set; }

    [FromForm]
    public IFormFile? File { get; set; }
}

public class LessonAttachmentCreateDto
{
#nullable disable
    public string FileUrl { get; set; }

    [JsonIgnore]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class SingleLessonAttachmentCreateDto
{
#nullable disable
    public int LessonId { get; set; }
    public string FileUrl { get; set; }

    [JsonIgnore]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class SingleLessonAttachmentFormDto
{
    [FromForm]
    public int LessonId { get; set; }

    [FromForm]
    public IFormFile AttachmentFile { get; set; } = null!;
}

public class CourseLessonAttachmenCreatetDto
{
#nullable disable
    public string FileUrl { get; set; }

    [JsonIgnore]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class CourseLessonAttachmentUpdateDto
{
#nullable enable
    public int? Id { get; set; } // if id is null, then add it as a new attachment

    [JsonIgnore]
    public string? FileUrl { get; set; }
}
