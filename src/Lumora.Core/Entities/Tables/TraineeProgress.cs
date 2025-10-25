namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class TraineeProgress : SharedData
    {
        public string? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? CourseId { get; set; }
        public CourseType? CourseType { get; set; }

        public int? ProgramId { get; set; }
        public virtual TrainingProgram? Program { get; set; }

        public ProgressLevel Level { get; set; }
        public bool IsCompleted { get; set; }
        public double CompletionPercentage { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
    }

    public enum ProgressLevel
    {
        Course,
        Program
    }
}
