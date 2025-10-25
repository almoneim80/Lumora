namespace Lumora.Extensions
{
    public static class GeneralResultExtensions
    {
        public static IActionResult ToActionResult(this GeneralResult result, Microsoft.Extensions.Logging.ILogger logger, bool logSuccess)
        {
            if (result.IsSuccess == true)
            {
                if (logSuccess)
                {
                    logger.LogInformation(result.Message);
                }

                return new OkObjectResult(result);
            }
            else
            {
                logger.LogError(result.Message);
                return result.ErrorType switch
                {
                    ErrorType.NotFound => new NotFoundObjectResult(new { result }),
                    ErrorType.Unauthorized => new UnauthorizedObjectResult(new { result }),
                    _ => new BadRequestObjectResult(new { result })
                };
            }
        }
    }
}
