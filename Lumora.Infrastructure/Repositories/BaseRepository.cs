using Lumora.Application.Interfaces.BaseIntf;
using System.Linq.Expressions;

namespace Lumora.Infrastructure.Repositories
{
    public class BaseRepository(PgDbContext dbContext) : IBaseRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            return await _dbContext.Set<TEntity>().AnyAsync(predicate);
        }

        public async Task AddAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
