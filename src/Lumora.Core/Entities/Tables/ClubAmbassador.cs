namespace Lumora.Entities.Tables
{
    public class ClubAmbassador : SharedData
    {
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public int? ClubPostId { get; set; }
        public virtual ClubPost? ClubPost { get; set; }

        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public string? Reason { get; set; }
    }
}
