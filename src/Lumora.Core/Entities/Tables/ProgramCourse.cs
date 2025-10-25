namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class ProgramCourse : SharedData, IBaseEntity
    {
#nullable disable
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public int ProgramId { get; set; }
        public virtual TrainingProgram TrainingProgram { get; set; } = null!;
        public string Logo { get; set; }

        public virtual ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
    }
}
