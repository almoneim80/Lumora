namespace Lumora.Entities;

// ====================== Interfaces ====================== //
public interface IHasCreatedAt
{
    public DateTimeOffset CreatedAt { get; set; }
}

public interface IHasUpdatedAt
{
    public DateTimeOffset? UpdatedAt { get; set; }
}

public interface IHasCreatedBy
{
    public string? CreatedByIp { get; set; }

    public string? CreatedById { get; set; }

    public string? CreatedByUserAgent { get; set; }
}

public interface IHasUpdatedBy
{
    public string? UpdatedByIp { get; set; }

    public string? UpdatedById { get; set; }

    public string? UpdatedByUserAgent { get; set; }
}

// ====================== Base Entities ====================== //

public class BaseEntityWithId
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Searchable]
    public string? Source { get; set; }

    public string? ById { get; set; }

    public string? ByIp { get; set; }

    public string? ByAgent { get; set; }
}

//public class BaseEntityWithIdAndDates : BaseEntityWithId, IHasCreatedAt, IHasUpdatedAt
//{
//    [Required]
//    public DateTime CreatedAt { get; set; } = DateTime.Now;

//    public DateTime? UpdatedAt { get; set; }
//}

public class BaseCreateByEntity : BaseEntityWithId, IHasCreatedAt, IHasCreatedBy
{
    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;

    public string? CreatedByIp { get; set; }

    public string? CreatedById { get; set; }

    public string? CreatedByUserAgent { get; set; }
}

public class BaseEntity : BaseCreateByEntity, IHasUpdatedAt, IHasUpdatedBy
{
    public DateTimeOffset? UpdatedAt { get; set; }

    public string? UpdatedByIp { get; set; }

    public string? UpdatedById { get; set; }

    public string? UpdatedByUserAgent { get; set; }
}

// ====================== Extended Shared Entity ====================== //

public class SharedData : BaseEntityWithId, ISharedData
{
    [JsonIgnore]
    public DateTimeOffset? DeletedAt { get; set; }

    [JsonIgnore]
    public DateTimeOffset? SoftDeleteExpiration { get; set; }

    [JsonIgnore]
    public bool IsDeleted { get; set; } = false;

    [JsonIgnore]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonIgnore]
    public DateTimeOffset? UpdatedAt { get; set; }
}
