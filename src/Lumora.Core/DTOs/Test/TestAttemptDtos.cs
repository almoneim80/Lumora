namespace Lumora.DTOs.Test;

public class TestAttemptStartDto
{
    public int AttemptId { get; set; }
    public int TestId { get; set; }
    public string TestTitle { get; set; } = null!;
    public DateTimeOffset StartedAt { get; set; }
    public int DurationInMinutes { get; set; }
}

public class TestAnswerDto
{
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int SelectedChoiceId { get; set; }
}

public class TestAttemptResultDto
{
    public int AttemptId { get; set; }
    public string UserId { get; set; } = null!;
    public string TestTitle { get; set; } = null!;
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }

    public List<TestAnswerReviewDto> Answers { get; set; } = new();
}

public class TestAnswerReviewDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public int SelectedChoiceId { get; set; }
    public string SelectedChoiceText { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

public class TestAttemptSummaryDto
{
    public int AttemptId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
}

public class StudentAttemptDto
{
    public int AttemptNumber { get; set; }
    public decimal Mark { get; set; }
    public string? Status { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset AttemptDate { get; set; }
}

public class StudentAttemptsWithBestDto
{
    public List<StudentAttemptDto> Attempts { get; set; } = new();
    public StudentAttemptDto? BestAttempt { get; set; }
}
