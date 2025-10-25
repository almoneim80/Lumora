namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class TestAnswer : SharedData
    {
        public int TestAttemptId { get; set; }
        public virtual TestAttempt TestAttempt { get; set; } = null!;

        public int TestQuestionId { get; set; }
        public virtual TestQuestion TestQuestion { get; set; } = null!;

        public int SelectedChoiceId { get; set; }
        public virtual TestChoice TestChoice { get; set; } = null!;

        public bool IsCorrect { get; set; }
    }
}
