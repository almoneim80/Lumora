namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class Test : SharedData
    {
        [ForeignKey("CourseLesson")]
        public int LessonId { get; set; }
        public virtual CourseLesson CourseLesson { get; set; } = null!;

        public string Title { get; set; } = null!;
        public int DurationInMinutes { get; set; }
        public decimal TotalMark { get; set; }
        public int MaxAttempts { get; set; }

        public virtual ICollection<TestQuestion> Questions { get; set; } = new List<TestQuestion>();
        public virtual ICollection<TestAttempt> Attempts { get; set; } = new List<TestAttempt>();
    }
}
