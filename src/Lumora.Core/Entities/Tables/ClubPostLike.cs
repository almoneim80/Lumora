namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class ClubPostLike : SharedData
    {
        public int ClubPostId { get; set; }
        public virtual ClubPost ClubPost { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
