namespace Lumora.Entities.Tables
{
    public class WheelPlayer : SharedData
    {
        public string PlayerId { get; set; } = null!;
        public virtual User Player { get; set; } = null!;

        public DateTimeOffset? PlayedAt { get; set; }

        public int AwardId { get; set; }
        public virtual WheelAward Award { get; set; } = null!;

        public bool IsFree { get; set; }

        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }

        public bool? IsDelivered { get; set; } = false;
    }
}
