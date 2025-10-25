namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class TestQuestion : SharedData
    {
#nullable disable
        public int TestId { get; set; }
        public virtual Test Test { get; set; } = null!;

        public string QuestionText { get; set; } = null!;
        public decimal Mark { get; set; }
        public int DisplayOrder { get; set; } = 1;

        public virtual ICollection<TestChoice> Choices { get; set; } = new List<TestChoice>();
    }
}

