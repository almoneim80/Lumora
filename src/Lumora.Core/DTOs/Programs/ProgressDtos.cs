namespace Lumora.DTOs.Programs;

/* =========================================== lesson progress =========================================== */
/* =========================================== lesson progress =========================================== */
public class LessonProgressDetailsDto
{
#nullable disable
    public int LessonId { get; set; }
    public string LessonName { get; set; }
    public string RelatedCourseName { get; set; }
    public string RelatedProgramName { get; set; }

#nullable disable
    public bool IsCompleted { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public TimeSpan TimeSpent { get; set; }
}

/// <summary>
/// Request for updating a course progress.
/// </summary>
public class CourseProgressDetailsDto
{
#nullable disable
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    public string RelatedProgramName { get; set; }
    public bool IsCompleted { get; set; }
    public double CompletionPercentage { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
}

/// <summary>
/// Request for updating a program progress.
/// </summary>
public class ProgramProgressDetailsDto
{
#nullable disable
    public int ProgramId { get; set; }
    public string ProgramName { get; set; }
    public bool IsCompleted { get; set; }
    public double CompletionPercentage { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
}

public class TrainingProgressCreateDto
{
    public int? CourseId { get; set; }
    public int? ProgramId { get; set; }
    public string UserId { get; set; }
}

public class StudentProgressDto
{
#nullable disable
    public string UserId { get; set; }
    public int CourseId { get; set; }
    public int LastLessonId { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public double CompletionPercentage { get; set; }
}

public class ProgramCompletionData
{
    public bool IsCompleted { get; set; }
    public double CompletionPercentage { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
}
