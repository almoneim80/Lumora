namespace Lumora.Entities.Tables
{
    public class WheelAward : SharedData
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Probability { get; set; }
        public AwardType Type { get; set; }
        public int? PointsAmount { get; set; }

        public virtual ICollection<WheelPlayer>? WheelPlayers { get; set; }
    }
}
