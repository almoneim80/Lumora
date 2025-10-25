namespace Lumora.Entities
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public virtual User? User { get; set; }

        [Required]
        public string TokenHash { get; set; } = null!;

        public DateTimeOffset Expiration { get; set; }

        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
