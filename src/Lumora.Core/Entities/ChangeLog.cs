namespace Lumora.Entities
{
    [Table("change_log")]
    public class ChangeLog : BaseEntityWithId, IHasCreatedAt
    {
        // name of the entity
        public string ObjectType { get; set; } = string.Empty;

        // id of the entity that was changed
        public int ObjectId { get; set; }

        // change type 
        public EntityState EntityState { get; set; }

        // old values before the change
        [Column(TypeName = "jsonb")]
        public string? OldValues { get; set; }

        // new values after the change
        [Column(TypeName = "jsonb")]
        public string? NewValues { get; set; }

        [Column(TypeName = "jsonb")]
        public string Data { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
    }
}
