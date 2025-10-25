using Lumora.Entities.Tables;

namespace Lumora.Data
{
    /// <summary>
    /// Club DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<ClubPost> ClubPosts { get; set; } = null!;
        public virtual DbSet<ClubPostLike> ClubPostLikes { get; set; } = null!;
        public virtual DbSet<ClubAmbassador> ClubAmbassadors { get; set; } = null!;
        public virtual DbSet<WheelAward> WheelAwards { get; set; } = null!;
        public virtual DbSet<WheelPlayerState> WheelPlayerStates { get; set; } = null!;
        public virtual DbSet<WheelPlayer> WheelPlayers { get; set; } = null!;
    }
}
