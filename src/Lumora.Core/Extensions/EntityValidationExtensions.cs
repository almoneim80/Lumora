namespace Up.Extensions
{
    public static class EntityValidationExtensions
    {
        public static async Task<IActionResult?> CheckIfEntityExistsAsync<T>(
            this int entityId,
            IExtendedBaseService baseService,
            Microsoft.Extensions.Logging.ILogger logger,
            string? notFoundMessage = null) where T : SharedData
        {
            var exists = await baseService.IsEntityExistsAndNotDeletedAsync<T>(entityId);
            if (!exists.IsSuccess)
            {
                var message = notFoundMessage ?? $"{typeof(T).Name} with ID {entityId} does not exist or has been deleted.";
                logger.LogWarning(message);

                var result = new GeneralResult<object>(false, message, null);
                return new BadRequestObjectResult(result);
            }

            return null;
        }
    }
}

