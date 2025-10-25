namespace Lumora.Entities.Tables
{
    [SupportsChangeLog]
    public class ClubPost : SharedData
    {
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public string Content { get; set; } = null!;
        public string? MediaUrl { get; set; }
        public MediaType MediaType { get; set; } = MediaType.None;

        public ClubPostStatus Status { get; set; } = ClubPostStatus.Pending;

        public DateTimeOffset? ApprovedAt { get; set; }
        public string? ApprovedById { get; set; }
        public string? Note { get; set; }
        
        public virtual ICollection<ClubPostLike> Likes { get; set; } = new HashSet<ClubPostLike>();
    }
}
