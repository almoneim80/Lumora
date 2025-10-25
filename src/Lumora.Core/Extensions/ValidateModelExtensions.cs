namespace Lumora.Extensions
{
    public static class ValidateModelExtensions
    {
        public static IActionResult? ValidateModelState(
            this ControllerBase controller,
            Microsoft.Extensions.Logging.ILogger logger,
            string? customMessage = null)
        {
            if (controller.ModelState.IsValid)
                return null;

            var message = customMessage ?? "Request model is not valid.";
            logger.LogError(message);

            var errors = controller.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

            var result = new GeneralResult<Dictionary<string, string[]>>(false, message, errors);
            return new BadRequestObjectResult(result);
        }
    }
}
