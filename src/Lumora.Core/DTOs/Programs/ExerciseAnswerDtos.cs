namespace Lumora.DTOs.Programs;

/// <summary>
/// DTO for submitting an answer.
/// </summary>
public class SubmitAnswerDto
{
#nullable disable
    [JsonIgnore]
    public string UserId { get; set; }
    public int ExerciseId { get; set; }
    public int SelectedChoiceId { get; set; }
}

/// <summary>
/// DTO for getting the review of an answer.
/// </summary>
public class AnswerReviewDto
{
#nullable enable
    public string Question { get; set; } = null!;
    public string SelectedChoice { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public string? CorrectChoice { get; set; }
}

/// <summary>
/// DTO for getting the score of a lesson.
/// </summary>
public class LessonScoreDto
{
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public double ScorePercentage { get; set; }
}
