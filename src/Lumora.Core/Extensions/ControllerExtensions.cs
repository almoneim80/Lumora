namespace Lumora.Extensions
{
    public static class ControllerExtensions
    {
        public static ObjectResult UnexpectedError(this ControllerBase controller, string actionName)
        {
            var message = $"Unexpected error occurred while {actionName}.";
            var result = new GeneralResult
            {
                IsSuccess = false,
                Message = message
            };

            return controller.StatusCode(StatusCodes.Status500InternalServerError, result);
        }
    }
}
