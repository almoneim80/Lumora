namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class CourseLesson : SharedData, IBaseEntity
    {
#nullable disable
        public string Name { get; set; }
        public string FileUrl { get; set; }
        public int Order { get; set; } // orginal order
        public int DurationInMinutes { get; set; }
        public string Description { get; set; }
        public int? OrderIndex { get; set; } // order after sorting

        public int ProgramCourseId { get; set; } 
        public virtual ProgramCourse ProgramCourse { get; set; } = null!;

        public virtual ICollection<LessonAttachment> LessonAttachments { get; set; } = new List<LessonAttachment>();
        public virtual Test LessonTest { get; set; }
    }
}
