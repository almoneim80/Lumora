namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class LessonAttachment : SharedData
    {
        public int LessonId { get; set; }
        public virtual CourseLesson CourseLesson { get; set; } = null!;

        public string FileUrl { get; set; } = null!;
        public int OpenCount { get; set; } = 0;
    }
}
