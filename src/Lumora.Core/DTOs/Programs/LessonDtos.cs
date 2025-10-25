using Lumora.DTOs.Test;

namespace Lumora.DTOs.Programs;

public class LessonUpdateDto
{
#nullable enable
    public int? CourseId { get; set; }
    public string? Name { get; set; }
    public string? FileUrl { get; set; }
    public int? Order { get; set; }
    public int? DurationInMinutes { get; private set; }
    public string? Description { get; set; }

    [JsonIgnore]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void SetDuration(int minutes)
    {
        DurationInMinutes = minutes;
    }
}

public class LessonUpdateFormDto
{
#nullable enable
    [FromForm]
    public int? CourseId { get; set; }

    [FromForm]
    public string? Name { get; set; }

    [FromForm]
    public int? Order { get; set; }

    [FromForm]
    public string? Description { get; set; }

    [FromForm]
    public IFormFile? LessonVideo { get; set; }
}

public class LessonFullDetailsDto
{
#nullable disable
    public int LessonId { get; set; }
    public string LessonName { get; set; }
    public string LessonFileUrl { get; set; }
    public int LessonOrder { get; set; }
    public int LessonDuration { get; set; }
    public string LessonDescription { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<AttachmentDetailsDto> Attachments { get; set; }
#nullable enable
    public TestDetailsDto? Test { get; set; }
}

public class LessonsWithContentCreateDto : LessonCreateDto
{
#nullable enable
    public List<LessonAttachmentCreateDto> Attachments { get; set; } = null!;
    public TestCreateDto? Test { get; set; }
}

public class LessonCreateDto
{
#nullable disable
    public string Name { get; set; }
    public string FileUrl { get; set; }
    public int Order { get; set; }

    [JsonIgnore]
    public int DurationInMinutes { get; private set; }
    public string Description { get; set; }

    [JsonIgnore]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void SetDuration(int minutes)
    {
        DurationInMinutes = minutes;
    }
}

public class CourseLessonCreateDto
{
#nullable disable
    public string Name { get; set; }
    public string FileUrl { get; set; }
    public int Order { get; set; }

    [JsonIgnore]
    public int DurationInMinutes { get; private set; }
    public string Description { get; set; }

    [JsonIgnore]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<CourseLessonAttachmenCreatetDto> LessonAttachments { get; set; }

    public void SetDuration(int minutes)
    {
        DurationInMinutes = minutes;
    }
}

public class CourseLessonUpdateDto
{
#nullable enable
    public int? Id { get; set; } // if id is null, then add it as a new lesson
    public string? Name { get; set; }

    [JsonIgnore]
    public string? FileUrl { get; set; }
    public int? Order { get; set; }

    [JsonIgnore]
    public int? DurationInMinutes { get; private set; }
    public string? Description { get; set; }

    public List<CourseLessonAttachmentUpdateDto>? LessonAttachments { get; set; }

    public void SetDuration(int minutes)
    {
        DurationInMinutes = minutes;
    }
}

public class LessonReorderDto
{
    public int LessonId { get; set; }
    public int OrderIndex { get; set; }
}

