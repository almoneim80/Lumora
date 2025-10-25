namespace Lumora.Entities.Tables
{
    public class UserLiveCourse : SharedData
    {
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public int LiveCourseId { get; set; }
        public virtual LiveCourse LiveCourse { get; set; } = null!;

        public int PaymentItemId { get; set; }
        public virtual PaymentItem? PaymentItem { get; set; } = null!;

        public DateTimeOffset RegisteredAt { get; set; }
    }
}
