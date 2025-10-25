namespace Lumora.Interfaces;
public interface IBaseEntity
{
    int Id { get; set; }
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    DateTimeOffset? SoftDeleteExpiration { get; set; }
}
