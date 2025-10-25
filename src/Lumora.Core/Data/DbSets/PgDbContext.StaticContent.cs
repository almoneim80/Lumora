namespace Lumora.Data
{
    /// <summary>
    /// StaticContent DbContext Class.
    /// </summary>
    public partial class PgDbContext
    {
        public virtual DbSet<StaticContent> StaticContents { get; set; } = null!;
    }
}
