namespace Lumora.DTOs.Test;

/******************************************* CREATE DTO ***************************************************/
/******************************************* CREATE DTO ***************************************************/
public class QuestionWithChoiseCreateDto
{
    public int TestId { get; set; }
    public string Question { get; set; } = null!;
    public decimal Mark { get; set; }

    public List<RelatedQuestionChoiceCreateDto> Choices { get; set; } = new();
}

public class RelatedQuestionChoiceCreateDto
{
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

/******************************************* UPDATE DTO ***************************************************/
/******************************************* UPDATE DTO ***************************************************/
public class TestQuestionUpdateDto
{
    public string? Question { get; set; } = null!;
    public decimal? Mark { get; set; }

    public List<RelatedQuestionChoiceUpdateDto>? Choices { get; set; } = new();
}

public class RelatedQuestionChoiceUpdateDto
{
    public int? Id { get; set; }
    public string? Text { get; set; } = null!;
    public bool? IsCorrect { get; set; }
}

/******************************************* DETAILS DTO ***************************************************/
/******************************************* DETAILS DTO ***************************************************/
public class QuestionDetailsDto
{
#nullable disable
    public int TestId { get; set; }
    public string TestTitle { get; set; }
    public int LessonId { get; set; }
    public string LessonName { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public decimal QuestionMark { get; set; }

    public List<RelatedQuestionChoiceDetailsDto> Choices { get; set; } = new List<RelatedQuestionChoiceDetailsDto>();
}

public class RelatedQuestionChoiceDetailsDto
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

public class ReorderDto
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
}
