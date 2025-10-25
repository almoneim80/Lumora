namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class LessonSession : SharedData
    {
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public int LessonId { get; set; }
        public virtual CourseLesson Lesson { get; set; } = null!;

        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? EndedAt { get; set; }
    }
}
