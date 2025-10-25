using Lumora.DTOs.TrainingProgram;

namespace Lumora.Interfaces;

public interface IStudentProgress
{
    /// <summary>
    /// Updates the progress of a student for a specific course.
    /// </summary>
    Task<bool> UpdateStudentProgressAsync(int courseId, string userId, int lessonId, TimeSpan timeSpent);

    /// <summary>
    /// Retrieves the progress of a student for a specific course.
    /// </summary>
    Task<StudentProgressDto> GetStudentProgressAsync(int courseId, string userId);

    /// <summary>
    /// Calculates the completion percentage of a student for a specific course.
    /// </summary>
    Task<double> CalculateCompletionPercentageAsync(int courseId, string userId);
}
