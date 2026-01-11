namespace Lumora.Application.Services.BaseSvc
{
    public class CascadeDeleteService(
        ICascadeRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CascadeDeleteService> logger,
        ICacheService cacheService,
        GeneralMessage messages,
        IIdentityRepository identityService) : ICascadeDeleteService
    {
        private readonly ICascadeRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<CascadeDeleteService> _logger = logger;
        private readonly ICacheService _cacheService = cacheService;
        private readonly GeneralMessage _messages = messages;
        private readonly IIdentityRepository _identityService = identityService;

        public async Task<GeneralResult<bool>> SoftDeleteCascadeAsync<T>(int id) where T : SharedData
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(default);
            try
            {
                var entity = await _repository.GetActiveEntityByIdAsync<T>(id);
                if (entity == null)
                    return new GeneralResult<bool>(false, _messages.MsgDataNotFound, false, ErrorType.NotFound);

                _repository.SoftDeleteRecursively(entity);

                var result = await _unitOfWork.SaveChangesAsync();
                if (result <= 0)
                {
                    await transaction.RollbackAsync(default);
                    return new GeneralResult<bool>(false, _messages.MsgFailedToDeleted, false, ErrorType.BadRequest);
                }

                await transaction.CommitAsync(default);
                await UpdateCacheAsync<T>(id);
                return new GeneralResult<bool>(true, _messages.MsgDataDeletedSuccessfully, true, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(default);
                _logger.LogError(ex, "Error cascading soft delete for {EntityType} ID {Id}", typeof(T).Name, id);
                return new GeneralResult<bool>(false, _messages.MsgFailedToDeleted, false, ErrorType.InternalServerError);
            }
        }

        public async Task<int> HardDeleteExpiredEntitiesAsync<TEntity>() where TEntity : class
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(default);
            try
            {
                int totalProcessed = 0;
                while (true)
                {
                    var expiredEntities = await _repository.GetExpiredEntitiesAsync<TEntity>(100);
                    if (!expiredEntities.Any()) break;

                    foreach (var entity in expiredEntities)
                    {
                        _repository.HardDeleteRelatedEntities(entity);
                        _repository.RemoveEntity(entity);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    totalProcessed += expiredEntities.Count;
                }

                await transaction.CommitAsync(default);
                return totalProcessed;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(default);
                _logger.LogError(ex, "Error in hard delete process for {Type}", typeof(TEntity).Name);
                throw;
            }
        }

        public async Task<bool> SoftDeleteUserCascadeAsync(string password, string userId, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var user = await _repository.GetActiveUserByIdAsync(userId);
                if (user == null) return false;

                var passwordValid = await _identityService.CheckPasswordAsync(user, password);
                if (!passwordValid.Succeeded) return false;

                _repository.SoftDeleteRecursively(user);

                var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
                if (result <= 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return false;
                }

                await transaction.CommitAsync(cancellationToken);
                await UpdateCacheAsync<User>(userId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error cascading soft delete for user ID {Id}", userId);
                throw;
            }
        }

        private async Task UpdateCacheAsync<T>(object id)
        {
            string cacheKeyForAll = $"{typeof(T).Name.ToLower()}_all";
            string cacheKeyForOne = $"{typeof(T).Name.ToLower()}_one_{id}";
            await _cacheService.RemoveAsync(cacheKeyForAll);
            await _cacheService.RemoveAsync(cacheKeyForOne);
        }
    }
}
