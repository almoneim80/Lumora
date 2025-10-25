namespace Lumora.DTOs.Test;

/******************************************* CREATE DTO ***************************************************/
/******************************************* CREATE DTO ***************************************************/
public class TestCreateDto
{
#nullable disable
    public int LessonId { get; set; }
    public string Title { get; set; }
    public int DurationInMinutes { get; set; }
    public int TotalMark { get; set; }
    public int MaxAttempts { get; set; }

    public List<RelatedTestQuestionDto> Questions { get; set; }
}

public class RelatedTestQuestionDto
{
    public string Text { get; set; }
    public decimal Mark { get; set; }

    public List<RelatedTestChoiceDto> Choices { get; set; }
}

public class RelatedTestChoiceDto
{
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
}

/******************************************* UPDATE DTO ***************************************************/
/******************************************* UPDATE DTO ***************************************************/
public class TestUpdateDto
{
#nullable enable
    public string? Title { get; set; }
    public int? DurationInMinutes { get; set; }
    public int? TotalMark { get; set; }
    public int? MaxAttempts { get; set; }

    public List<RelatedTestQuestionUpdateDto>? Questions { get; set; } = new();
}

public class RelatedTestQuestionUpdateDto
{
    public int? Id { get; set; }
    public string? Question { get; set; }
    public decimal? Mark { get; set; }

    public List<RelatedTestChoiceUpdateDto>? Choices { get; set; } = new();
}

public class RelatedTestChoiceUpdateDto
{
    public int? Id { get; set; }
    public string? Text { get; set; }
    public bool? IsCorrect { get; set; }
}

/******************************************* DETAILS DTO ***************************************************/
/******************************************* DETAILS DTO ***************************************************/

public class TestDetailsDto
{
#nullable disable
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string LessonName { get; set; }
    public string Title { get; set; } = null!;
    public int DurationInMinutes { get; set; }
    public decimal TotalMark { get; set; }
    public int MaxAttempts { get; set; }

    public List<RelatedTestQuestionDetailsDto> Questions { get; set; } = new();
}

public class RelatedTestQuestionDetailsDto
{
    public int Id { get; set; }
    public string Question { get; set; } = null!;
    public decimal Mark { get; set; }
    public int DisplayOrder { get; set; }

    public List<RelatedTestChoiceDetailsDto> Choices { get; set; } = new();
}

public class RelatedTestChoiceDetailsDto
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
}

/******************************************* ADD WITH FILES DTO ***************************************************/
/******************************************* ADD WITH FILES DTO ***************************************************/
public class CourseWithLessonsCreateFormDto
{
#nullable enable
    [FromForm]
    public string? CourseJson { get; set; }

    [FromForm(Name = "CourseLogo")]
    public IFormFile? CourseLogo { get; set; }
}

public class LessonWithContentCreateFormDto
{
    [Required]
    public string LessonJson { get; set; } = null!;

    public IFormFile? LessonVideo { get; set; }

    public List<IFormFile>? Attachments { get; set; }
}
