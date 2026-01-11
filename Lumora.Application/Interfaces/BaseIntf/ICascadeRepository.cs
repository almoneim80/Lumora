namespace Lumora.Application.Interfaces.BaseIntf
{
    public interface ICascadeRepository
    {
        Task<T?> GetActiveEntityByIdAsync<T>(int id) where T : SharedData;
        void SoftDeleteRecursively<T>(T entity) where T : class;
        Task<List<TEntity>> GetExpiredEntitiesAsync<TEntity>(int batchSize) where TEntity : class;
        void HardDeleteRelatedEntities<TEntity>(TEntity entity) where TEntity : class;
        void RemoveEntity<TEntity>(TEntity entity) where TEntity : class;
        Task<User?> GetActiveUserByIdAsync(string userId);
    }
}
