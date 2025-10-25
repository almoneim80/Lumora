namespace Lumora.Data
{
    /// <summary>
    /// Test DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<Test> Tests { get; set; } = null!;
        public virtual DbSet<TestQuestion> TestQuestions { get; set; } = null!;
        public virtual DbSet<TestChoice> TestChoices { get; set; } = null!;
        public virtual DbSet<TestAnswer> TestAnswers { get; set; } = null!;
        public virtual DbSet<TestAttempt> TestAttempts { get; set; } = null!;
    }
}
