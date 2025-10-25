namespace Lumora.DTOs.Test;

public class ChoiceCreateDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

public class ChoiceUpdateDto
{
    public string? Text { get; set; }
    public bool? IsCorrect { get; set; }
}

public class ChoiceDetailsDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}
