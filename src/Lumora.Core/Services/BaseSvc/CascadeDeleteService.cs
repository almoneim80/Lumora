using Microsoft.AspNetCore.Identity;

namespace Lumora.Services.BaseSvc
{
    public class CascadeDeleteService(
        PgDbContext dbContext,
        ILogger<CascadeDeleteService> logger,
        ICacheService cacheService,
        GeneralMessage messages,
        UserManager<User> userManager) : ICascadeDeleteService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<CascadeDeleteService> _logger = logger;
        private readonly ICacheService _cacheService = cacheService;
        private readonly GeneralMessage _messages = messages;
        private readonly UserManager<User> _userManager = userManager;

        /// <inheritdoc/>
        public async Task<GeneralResult<bool>> SoftDeleteCascadeAsync<T>(int id) where T : SharedData
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
                if (entity == null)
                    return new GeneralResult<bool>(false, _messages.MsgDataNotFound, false, ErrorType.NotFound);

                SoftDeleteRecursively(entity, _dbContext);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResult<bool>(false, _messages.MsgFailedToDeleted, false, ErrorType.BadRequest);
                }

                await transaction.CommitAsync();
                UpdateCache<T>(id);
                return new GeneralResult<bool>(true, _messages.MsgDataDeletedSuccessfully, true, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during cascading soft delete for entity {EntityType} with ID {Id}", typeof(T).Name, id);
                return new GeneralResult<bool>(false, _messages.MsgFailedToDeleted, false, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<int> HardDeleteExpiredEntitiesAsync<TEntity>()
         where TEntity : class
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                const int batchSize = 100;
                int totalProcessed = 0;

                while (true)
                {
                    var expiredEntities = await _dbContext.Set<TEntity>()
                        .IgnoreQueryFilters()
                        .Where(e => EF.Property<DateTimeOffset>(e, "SoftDeleteExpiration") <= DateTimeOffset.UtcNow
                                 && EF.Property<bool>(e, "IsDeleted") == true)
                        .OrderBy(e => EF.Property<DateTimeOffset>(e, "SoftDeleteExpiration"))
                        .Take(batchSize)
                        .ToListAsync();

                    if (!expiredEntities.Any())
                        break;

                    foreach (var entity in expiredEntities)
                    {
                        var navigationProperties = _dbContext.Entry(entity)
                            .Navigations
                            .Where(n => n.Metadata.IsCollection)
                            .ToList();

                        foreach (var navigation in navigationProperties)
                        {
                            if (!navigation.IsLoaded)
                            {
                                await navigation.LoadAsync();
                            }

                            if (navigation.CurrentValue is IEnumerable<object> relatedEntities)
                            {
                                _dbContext.RemoveRange(relatedEntities);
                            }
                        }

                        _dbContext.Set<TEntity>().Remove(entity);
                    }

                    await _dbContext.SaveChangesAsync();
                    totalProcessed += expiredEntities.Count;

                    _logger.LogInformation($"Processed {totalProcessed} entities so far...");
                }

                await transaction.CommitAsync();

                _logger.LogInformation($"Hard delete completed. Total processed entities: {totalProcessed}.");
                return totalProcessed;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred during the hard delete process.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SoftDeleteUserCascadeAsync(string password, string userId, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                if (user == null) return false;

                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid) return false;

                SoftDeleteRecursivelyUser(user, _dbContext);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0) await transaction.RollbackAsync();

                await transaction.CommitAsync();

                // update cache
                string cacheKeyForAll = $"{typeof(User).Name.ToLower()}_all";
                string cacheKeyForOne = $"{typeof(User).Name.ToLower()}_one_{user.Id}";
                await _cacheService.RemoveAsync(cacheKeyForAll);
                await _cacheService.RemoveAsync(cacheKeyForOne);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred during cascading soft delete for user with ID {Id}", userId);
                throw;
            }
        }

        #region Private Methods
        private void SoftDeleteRecursively(SharedData entity, DbContext dbContext)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTimeOffset.UtcNow;
            //entity.SoftDeleteExpiration = DateTimeOffset.UtcNow;

            var entry = dbContext.Entry(entity);
            var collectionNavigations = entry.Metadata.GetNavigations().Where(nav => nav.IsCollection);

            foreach (var nav in collectionNavigations)
            {
                entry.Collection(nav.Name).Load();
            }

            var collectionProps = entity.GetType()
                .GetProperties()
                .Where(p => typeof(IEnumerable<SharedData>).IsAssignableFrom(p.PropertyType));

            foreach (var prop in collectionProps)
            {
                var children = prop.GetValue(entity) as IEnumerable<SharedData>;
                if (children == null)
                    continue;

                foreach (var child in children)
                {
                    SoftDeleteRecursively(child, dbContext);
                }
            }

            // update cache
            UpdateCache(entity);
        }
        private void SoftDeleteRecursivelyUser(User user, DbContext dbContext)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTimeOffset.UtcNow;
            user.SoftDeleteExpiration = DateTimeOffset.UtcNow;

            // upload child entities
            var entry = dbContext.Entry(user);
            var collectionNavigations = entry.Metadata.GetNavigations()
                .Where(nav => nav.IsCollection);

            // upload collection entities
            foreach (var nav in collectionNavigations)
            {
                entry.Collection(nav.Name).Load();
            }

            // search for collection properties
            var collectionProps = user.GetType()
                .GetProperties()
                .Where(p => typeof(IEnumerable<User>).IsAssignableFrom(p.PropertyType)
                          || typeof(IEnumerable<SharedData>).IsAssignableFrom(p.PropertyType)
                          || typeof(IEnumerable<ISharedData>).IsAssignableFrom(p.PropertyType));

            foreach (var prop in collectionProps)
            {
                var children = prop.GetValue(user) as IEnumerable<object>;
                if (children == null) continue;

                foreach (var child in children)
                {
                    if (child is User childUser)
                    {
                        // recursion
                        SoftDeleteRecursivelyUser(childUser, dbContext);
                    }
                    else if (child is SharedData sharedChild)
                    {
                        SoftDeleteRecursively(sharedChild, dbContext);
                    }
                }
            }

            // update cache
            UpdateCache(user);
        }
        private void UpdateCache<TEntity>(int id)
        where TEntity : class
        {
            string cacheKeyForAll = $"{typeof(TEntity).Name.ToLower()}_all";
            string cacheKeyForOne = $"{typeof(TEntity).Name.ToLower()}_one_{id}";

            _cacheService.RemoveAsync(cacheKeyForAll);
            _cacheService.RemoveAsync(cacheKeyForOne);
        }
        private void UpdateCache(SharedData entity)
        {
            string cacheKeyForAll = $"{entity.GetType().Name.ToLower()}_all";
            string cacheKeyForOne = $"{entity.GetType().Name.ToLower()}_one_{entity.Id}";

            _cacheService.RemoveAsync(cacheKeyForAll);
            _cacheService.RemoveAsync(cacheKeyForOne);

            _logger.LogInformation("Removed cache for entity type {EntityType} with ID {Id}", entity.GetType().Name, entity.Id);
        }
        private void UpdateCache(User user)
        {
            string cacheKeyForAll = $"{user.GetType().Name.ToLower()}_all";
            string cacheKeyForOne = $"{user.GetType().Name.ToLower()}_one_{user.Id}";

            _cacheService.RemoveAsync(cacheKeyForAll);
            _cacheService.RemoveAsync(cacheKeyForOne);

            _logger.LogInformation("Removed cache for user with ID {Id}", user.Id);
        }
        #endregion
    }
}
