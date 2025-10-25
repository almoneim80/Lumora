namespace Lumora.DTOs.TrainingProgram;
public class CourseWithLessonsCreateDto
{
#nullable disable
    public int ProgramId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public string Logo { get; set; }

    [JsonIgnore]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<LessonsWithContentCreateDto> Lessons { get; set; }
}

public class CourseUpdateFormDto
{
#nullable enable

    [FromForm]
    public int? ProgramId { get; set; }

    [FromForm]
    public string? Name { get; set; }

    [FromForm]
    public string? Description { get; set; }

    [FromForm]
    public int? Order { get; set; }

    [FromForm]
    public IFormFile? Logo { get; set; }
}

public class CourseUpdateDto
{
#nullable enable
    public int? ProgramId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Order { get; set; }
    public string? Logo { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CourseFullDetailsDto
{
#nullable disable
    public int ProgramId { get; set; }
    public string ProgramName { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    public string CourseDescription { get; set; }
    public int CourseOrder { get; set; }
    public string CourseLogo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<LessonFullDetailsDto> Lessons { get; set; }
}

public class ProgramCourseCreateDto
{
#nullable disable
    public string Name { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public string Logo { get; set; }

    [JsonIgnore]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<CourseLessonCreateDto> Lessons { get; set; }
}

public class ProgramCourseUpdateDto
{
#nullable enable
    public int? Id { get; set; } // if id is null, then add it as a new course
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Order { get; set; }
    public string? Logo { get; set; }
    public List<CourseLessonUpdateDto>? Lessons { get; set; }

    [JsonIgnore]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class CourseStatisticsDto
{
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public int StudentCount { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
}
