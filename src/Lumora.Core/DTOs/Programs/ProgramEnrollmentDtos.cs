namespace Lumora.DTOs.TrainingProgram;

/// <summary>
/// used to return users data that enrolled in Course.
/// </summary>
public class EnrollmentWithUserData
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset? EnrolledAt { get; set; }
    public EnrollmentStatus EnrollmentStatus { get; set; }
}
