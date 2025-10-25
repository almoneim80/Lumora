namespace Lumora.Interfaces
{
    public interface IEntityService<T>
        where T : BaseEntityWithId
    {
        Task SaveAsync(T item);

        Task SaveRangeAsync(List<T> items);

        void SetDBContext(PgDbContext pgDbContext);
    }
}
