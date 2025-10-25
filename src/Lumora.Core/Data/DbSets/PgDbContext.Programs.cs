namespace Lumora.Data
{
    /// <summary>
    /// Training Programs DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<TrainingProgram> TrainingPrograms { get; set; } = null!;
        public virtual DbSet<ProgramEnrollment> ProgramEnrollments { get; set; } = null!;
        public virtual DbSet<ProgramCertificate> ProgramCertificates { get; set; } = null!;
        public virtual DbSet<TraineeProgress> TraineeProgresses { get; set; } = null!;
        public virtual DbSet<LessonProgress> LessonProgresses { get; set; } = null!;
        public virtual DbSet<ProgramCourse> ProgramCourses { get; set; } = null!;
        public virtual DbSet<CourseLesson> CourseLessons { get; set; } = null!;
        public virtual DbSet<LessonSession> LessonSessions { get; set; } = null!;
        public virtual DbSet<LessonAttachment> LessonAttachments { get; set; } = null!;
    }
}
