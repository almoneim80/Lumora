using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Lumora.Interfaces.Customer;

namespace Lumora.Identity
{
    public class GoogleCookieEventsHandler : CookieAuthenticationEvents
    {
        public override async Task SigningIn(CookieSigningInContext context)
        {
            var userService = context.HttpContext.RequestServices.GetService<IUserService>()!;
            var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<User>>()!;
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>()!;

            var phoneNumber = context.Principal?.Claims.FirstOrDefault(claim => claim.Type == "phoneNumber")?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                await RedirectToAccessDenied(((PropertiesContext<CookieAuthenticationOptions>)context as RedirectContext<CookieAuthenticationOptions>)!);
                return;
            }

            var user = await userService.FindOnRegister(phoneNumber);

            user.LastTimeLoggedIn = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);

            await signInManager.SignInAsync(user, false, "GoogleCookie");

            context.HttpContext.User = userPrincipal;
        }
    }
}
