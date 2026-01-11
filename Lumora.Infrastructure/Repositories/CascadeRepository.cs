using Lumora.Application.Interfaces.BaseIntf;

namespace Lumora.Infrastructure.Repositories
{
    public class CascadeRepository(PgDbContext dbContext) : ICascadeRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<T?> GetActiveEntityByIdAsync<T>(int id) where T : SharedData
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<User?> GetActiveUserByIdAsync(string userId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        public void SoftDeleteRecursively<T>(T entity) where T : class
        {
            ApplySoftDelete(entity);

            var entry = _dbContext.Entry(entity);
            var navigations = entry.Metadata.GetNavigations().Where(n => n.IsCollection);

            foreach (var nav in navigations)
            {
                if (!entry.Collection(nav.Name).IsLoaded)
                    entry.Collection(nav.Name).Load();

                var children = entry.Collection(nav.Name).CurrentValue as IEnumerable<object>;
                if (children == null) continue;

                foreach (var child in children)
                {
                    SoftDeleteRecursively(child);
                }
            }
        }

        private void ApplySoftDelete(object entity)
        {
            if (entity is SharedData sd)
            {
                sd.IsDeleted = true;
                sd.DeletedAt = DateTimeOffset.UtcNow;
            }
            else if (entity is User u)
            {
                u.IsDeleted = true;
                u.DeletedAt = DateTimeOffset.UtcNow;
                u.SoftDeleteExpiration = DateTimeOffset.UtcNow;
            }
        }

        public async Task<List<TEntity>> GetExpiredEntitiesAsync<TEntity>(int batchSize) where TEntity : class
        {
            return await _dbContext.Set<TEntity>()
                .IgnoreQueryFilters()
                .Where(e => EF.Property<DateTimeOffset>(e, "SoftDeleteExpiration") <= DateTimeOffset.UtcNow
                         && EF.Property<bool>(e, "IsDeleted") == true)
                .OrderBy(e => EF.Property<DateTimeOffset>(e, "SoftDeleteExpiration"))
                .Take(batchSize)
                .ToListAsync();
        }

        public void HardDeleteRelatedEntities<TEntity>(TEntity entity) where TEntity : class
        {
            var entry = _dbContext.Entry(entity);
            var navigations = entry.Metadata.GetNavigations().Where(n => n.IsCollection);

            foreach (var navigation in navigations)
            {
                if (!entry.Collection(navigation.Name).IsLoaded)
                    entry.Collection(navigation.Name).Load();

                var relatedEntities = entry.Collection(navigation.Name).CurrentValue as IEnumerable<object>;
                if (relatedEntities != null)
                {
                    _dbContext.RemoveRange(relatedEntities);
                }
            }
        }

        public void RemoveEntity<TEntity>(TEntity entity) where TEntity : class
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }
    }
}
