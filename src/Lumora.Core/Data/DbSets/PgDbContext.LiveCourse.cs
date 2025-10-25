namespace Lumora.Data
{
    /// <summary>
    /// Live Course DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<LiveCourse> LiveCourses { get; set; } = null!;
        public virtual DbSet<UserLiveCourse> UserLiveCourses { get; set; } = null!;
    }
}
