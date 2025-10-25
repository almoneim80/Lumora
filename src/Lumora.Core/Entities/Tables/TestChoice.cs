namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class TestChoice : SharedData
    {
#nullable disable
        public int TestQuestionId { get; set; }
        public virtual TestQuestion TestQuestion { get; set; } = null!;

        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public int DisplayOrder { get; set; } = 1;

        public virtual ICollection<TestAnswer> Answers { get; set; } = new HashSet<TestAnswer>();
    }
}
