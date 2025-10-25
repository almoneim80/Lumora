namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class LiveCourse : SharedData
    {
        public string Title { get; set; } = default!;
        public decimal Price { get; set; }
        public string Description { get; set; } = default!;
        public string ImagePath { get; set; } = default!;
        public bool IsActive { get; set; }
        public string StudyWay { get; set; } = default!;
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string Link { get; set; } = default!;
        public string? Lecturer { get; set; }
        public virtual ICollection<UserLiveCourse> UserLiveCourses { get; set; } = new List<UserLiveCourse>();
    }
}
