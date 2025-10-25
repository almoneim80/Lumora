namespace Lumora.Interfaces
{
    public interface ICascadeDeleteService
    {
        /// <summary>
        /// Soft deletes a user and its related entities with cascading soft delete.
        /// </summary>
        Task<bool> SoftDeleteUserCascadeAsync(string password, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Hard deletes entities that are expired.
        /// </summary>
        Task<int> HardDeleteExpiredEntitiesAsync<TEntity>() where TEntity : class;

        /// <summary>
        /// Soft deletes an entity and its related entities with cascading soft delete.
        /// </summary>
        Task<GeneralResult<bool>> SoftDeleteCascadeAsync<T>(int id) where T : SharedData;
    }
}
