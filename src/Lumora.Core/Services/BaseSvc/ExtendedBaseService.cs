namespace Lumora.Services.BaseSvc
{
    public class ExtendedBaseService(PgDbContext dbContext, ILogger<ExtendedBaseService> logger, ILocalizationManager localizationManager) : IExtendedBaseService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<ExtendedBaseService> _logger = logger;
        private readonly ILocalizationManager _localizationManager = localizationManager;

        //// <inheritdoc />
        public async Task<GeneralResult<bool>> IsEntityExistsAndNotDeletedAsync<TEntity>(int entityId)
        where TEntity : SharedData
        {
            try
            {
                var exists = await _dbContext.Set<TEntity>().AnyAsync(e => e.Id == entityId && !e.IsDeleted);
                if (!exists)
                {
                    _logger.LogWarning("Entity {EntityType} with ID {EntityId} either does not exist or is soft-deleted.", typeof(TEntity).Name, entityId);
                    return new GeneralResult<bool>(false, "Entity does not exist or is soft-deleted.", false);
                }

                return new GeneralResult<bool>(true, _localizationManager.GetLocalizedString("EntityExistsAndNotDeleted"), exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking existence of entity {EntityType} with ID {EntityId}.", typeof(TEntity).Name, entityId);
                return new GeneralResult<bool>(false, _localizationManager.GetLocalizedString("ErrorCheckingEntity"), false);
            }
        }

        //// <inheritdoc />
        public async Task AddEntityAsync<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        //// <inheritdoc />
        public IEnumerable<EnumData> GetEnumValues<T>() where T : Enum
        {
            var enumData = Enum.GetValues(typeof(T)).Cast<T>()
                            .Select(e => new EnumData
                            {
                                Value = Convert.ToInt32(e),
                                Description = GetEnumDescription(e)
                            })
                            .ToList();

            return enumData;
        }

        /// <summary>
        /// Returns the description of the enum value if it has one attached to it or the enum value itself if it does not.
        /// </summary>
        private string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString())!;

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
