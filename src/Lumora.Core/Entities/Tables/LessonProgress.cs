namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class LessonProgress : SharedData
    {
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public int LessonId { get; set; }
        public virtual CourseLesson Lesson { get; set; } = null!;

        public bool IsCompleted { get; set; } 
        public DateTimeOffset CompletedAt { get; set; }
        public TimeSpan TimeSpent { get; set; } = TimeSpan.Zero;
    }
}
