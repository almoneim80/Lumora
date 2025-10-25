using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Lumora.Configuration;
using Lumora.Identity;

namespace Lumora.Infrastructure;

public static class IdentityHelper
{
    public static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        ConfigureIdentity(builder);

        var cookiesConfig = builder.Configuration.GetSection("Cookies").Get<CookiesConfig>();

        if (cookiesConfig != null && cookiesConfig.Enable)
        {
            ConfigureCookies(builder, cookiesConfig);
        }

        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>();

        if (jwtConfig != null && jwtConfig.Secret != "$JWT__SECRET")
        {
            ConfigureInternalJwt(authBuilder, jwtConfig);
        }

        // Add Google authentication
        authBuilder.AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
            googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            googleOptions.CallbackPath = new PathString("/signin-google");
        });
    }

    public static void ConfigureCookies(WebApplicationBuilder builder, CookiesConfig cookiesConfig)
    {
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(cookiesConfig.ExpireTime);
            options.Cookie.Name = cookiesConfig.Name;

            options.LoginPath = "/api/identity/external-login";
            options.AccessDeniedPath = "/access-denied";
            options.SlidingExpiration = true;
        });

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.None;
            options.Secure = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        });
    }

    public static void ConfigureIdentity(WebApplicationBuilder builder)
    {
        var identityConfig = builder.Configuration.GetSection("Identity").Get<IdentityConfig>();

        builder.Services.AddIdentity<User, IdentityRole>(options =>
        {
            if (identityConfig != null)
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityConfig.LockoutTime);
                options.Lockout.MaxFailedAccessAttempts = identityConfig.MaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = true;
            }

            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<PgDbContext>()
        .AddDefaultTokenProviders()
        .AddUserManager<UserManager<User>>()
        .AddSignInManager<CustomSignInManager<User>>();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = string.Empty;
        });

        builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromHours(1));

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
        });

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
        });

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        });

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        });
    }

    public static void ConfigureInternalJwt(AuthenticationBuilder authBuilder, JwtConfig jwtConfig)
    {
        authBuilder.AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = jwtConfig.Audience,
                ValidIssuer = jwtConfig.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
            };
        });
    }
}
