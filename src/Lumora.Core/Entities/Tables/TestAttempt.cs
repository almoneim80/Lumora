namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class TestAttempt : SharedData
    {
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public int TestId { get; set; }
        public virtual Test Test { get; set; } = null!;

        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }

        public decimal TotalMark { get; set; }
        public bool IsPassed { get; set; }
        public bool IsValidSubmission { get; set; }

        public virtual ICollection<TestAnswer> Answers { get; set; } = new List<TestAnswer>();
    }
}
