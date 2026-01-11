namespace Lumora.Application.Services.BaseSvc
{
    public class ExtendedBaseService(
            IBaseRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<ExtendedBaseService> logger,
            ILocalizationManager localizationManager) : IExtendedBaseService
    {
        private readonly IBaseRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<ExtendedBaseService> _logger = logger;
        private readonly ILocalizationManager _localizationManager = localizationManager;

        //// <inheritdoc />
        public async Task<GeneralResult<bool>> IsEntityExistsAndNotDeletedAsync<TEntity>(int entityId)
            where TEntity : SharedData
        {
            try
            {
                // استخدام الـ Repository بدلاً من DbContext مباشرة
                var exists = await _repository.AnyAsync<TEntity>(e => e.Id == entityId && !e.IsDeleted);

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
            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(); // استخدام UnitOfWork لضمان تناسق الحفظ
        }

        //// <inheritdoc />
        public IEnumerable<EnumData> GetEnumValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>()
                .Select(e => new EnumData
                {
                    Value = Convert.ToInt32(e),
                    Description = GetEnumDescription(e)
                })
                .ToList();
        }

        private string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString())!;
            if (fi == null) return value.ToString();

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes != null && attributes.Length > 0)
                ? attributes[0].Description
                : value.ToString();
        }
    }
}
