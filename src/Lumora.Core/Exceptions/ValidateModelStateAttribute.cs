using Microsoft.AspNetCore.Mvc.Filters;

namespace Lumora.Exceptions
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var firstError = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => e.Value!.Errors.First().ErrorMessage)
                    .FirstOrDefault() ?? "Invalid input.";

                var result = new GeneralResult(
                    success: false,
                    message: firstError,
                    data: null,
                    errorType: ErrorType.Validation);

                context.Result = new BadRequestObjectResult(result);
            }
        }
    }
}
