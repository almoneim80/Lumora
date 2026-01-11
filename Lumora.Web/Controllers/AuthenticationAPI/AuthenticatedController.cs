namespace Lumora.Web.Controllers.AuthenticationAPI
{
    [ApiController]
    [Authorize]
    public abstract class AuthenticatedController : ControllerBase
    {
        protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        protected bool IsUserLoggedIn => !string.IsNullOrEmpty(CurrentUserId);

        protected IActionResult? CheckUserOrUnauthorized()
        {
            return IsUserLoggedIn ? null : Unauthorized(new GeneralResult { IsSuccess = false, Message = "User not logged in." });
        }
    }
}
