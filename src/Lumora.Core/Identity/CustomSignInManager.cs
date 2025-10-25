using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Lumora.Identity
{
    public class CustomSignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        public CustomSignInManager(UserManager<TUser> userManager,
                                   IHttpContextAccessor contextAccessor,
                                   IUserClaimsPrincipalFactory<TUser> claimsFactory,
                                   IOptions<IdentityOptions> optionsAccessor,
                                   ILogger<SignInManager<TUser>> logger,
                                   IAuthenticationSchemeProvider schemes,
                                   IUserConfirmation<TUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public override async Task<bool> CanSignInAsync(TUser user)
        {
            if (Options.SignIn.RequireConfirmedPhoneNumber && !(await UserManager.IsPhoneNumberConfirmedAsync(user)))
            {
                Logger.LogDebug($"User {user}, cannot sign in without a confirmed phone number. ");
                return false;
            }

            return true;
        }
    }
}
