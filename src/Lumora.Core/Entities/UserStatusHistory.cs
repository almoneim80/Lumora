namespace Lumora.Entities
{
    public class UserStatusHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public bool NewStatus { get; set; }
        public string Reason { get; set; } = null!;
        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

        // navigation property
        public virtual User? User { get; set; }
    }
}
