namespace Lumora.Interfaces
{
    public interface IExtendedBaseService
    {
        Task<GeneralResult<bool>> IsEntityExistsAndNotDeletedAsync<TEntity>(int entityId)
            where TEntity : SharedData;
        Task AddEntityAsync<T>(T entity) where T : class;
        IEnumerable<EnumData> GetEnumValues<T>() where T : Enum;
    }
}
