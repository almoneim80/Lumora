using System.Text.Json;
using Lumora.Configuration;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Quartz.Impl.AdoJobStore.Common;
using Lumora.Interfaces.Token;
using Lumora.Interfaces.Authorization;
using Lumora.DTOs.Token;

namespace Lumora.Services;

/// <summary>
/// Implementation of IExternalAuthService for Google OAuth 2.0 authentication.
/// Handles Google login, user registration, and token management.
/// </summary>
public class GoogleAuthService : IExternalAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly HttpClient _httpClient;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRoleService _roleService;
    private readonly DefaultRolesConfig _defaultRoles;
    protected readonly PgDbContext _dbContext;
    private readonly IMapper _mapper;

    public GoogleAuthService(
        IConfiguration configuration,
        ILogger<GoogleAuthService> logger,
        HttpClient httpClient,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IRoleService roleService,
        DefaultRolesConfig defaultRoles,
        PgDbContext dbContext,
        IMapper mapper)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _roleService = roleService;
        _defaultRoles = defaultRoles;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <summary>
    /// Generates the Google login URL with specified scopes.
    /// </summary>
    /// <param name="requestedScopes">A list of requested scopes for Google authentication.</param>
    /// <returns>A URL to redirect the user to Google's login page.</returns>
    public string GenerateGoogleLoginUrl(List<string> requestedScopes)
    {
        try
        {
            if (requestedScopes == null || !requestedScopes.Any())
            {
                throw new ArgumentException("Requested scopes cannot be null or empty.");
            }

            string clientId = _configuration["Authentication:Google:ClientId"]!;
            string redirectUri = _configuration["Authentication:Google:RedirectUri"]!;
            string scopes = string.Join(" ", requestedScopes);

            string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                             $"client_id={Uri.EscapeDataString(clientId)}&" +
                             $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                             "response_type=code&" +
                             $"scope={Uri.EscapeDataString(scopes)}&" +
                             "access_type=offline&prompt=consent";

            _logger.LogInformation("Generated Google login URL.");
            return authUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Google login URL.");
            throw;
        }
    }

    /// <summary>
    /// Handles the Google authentication callback.
    /// </summary>
    /// <param name="code">The authorization code returned by Google.</param>
    /// <returns>An AuthResult containing the status of the authentication.</returns>
    public async Task<AuthResult> HandleGoogleAuthCallbackAsync(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Authorization code cannot be null or empty.");
            }

            string tokenEndpoint = "https://oauth2.googleapis.com/token";
            string clientId = _configuration["Authentication:Google:ClientId"]!;
            string clientSecret = _configuration["Authentication:Google:ClientSecret"]!;
            string redirectUri = _configuration["Authentication:Google:RedirectUri"]!;

            var tokenRequestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            HttpResponseMessage tokenResponse = await _httpClient.PostAsync(tokenEndpoint, tokenRequestContent);
            tokenResponse.EnsureSuccessStatusCode();

            string tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Token response received: {TokenContent}", tokenContent);

            var tokenResponseData = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenContent);

            if (string.IsNullOrEmpty(tokenResponseData?.AccessToken))
            {
                throw new InvalidOperationException("Access token is null or empty. Please try again.");
            }

            string userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponseData.AccessToken);

            HttpResponseMessage userInfoResponse = await _httpClient.GetAsync(userInfoEndpoint);
            userInfoResponse.EnsureSuccessStatusCode();

            string userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("User info response received: {UserInfoContent}", userInfoContent);

            var userInfoData = JsonSerializer.Deserialize<GoogleUserInfoResponse>(userInfoContent);

            if (userInfoData == null || string.IsNullOrWhiteSpace(userInfoData.Email))
            {
                throw new InvalidOperationException("Failed to retrieve user information from Google.");
            }

            var userFromDb = await _userManager.FindByEmailAsync(userInfoData.Email);

            if (userFromDb == null)
            {
                var externalRegisterDto = _mapper.Map<ExternalRegisterDto>(userInfoData);
                var result = await ExternalRegister(externalRegisterDto);

                if (!result.Success)
                {
                    return new AuthResult { Success = false, ErrorMessage = result.ErrorMessage };
                }

                var newUser = _mapper.Map<User>(result.UserInfo);
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                var token = await _tokenService.GenerateTokenWithRefreshTokenAsync(newUser);

                var newAuthResult = new AuthResult { Success = true, Token = token.AccessToken, Email = newUser.Email, ErrorMessage = "User authenticated successfully" };
                return newAuthResult;
            }

            userFromDb.FullName = userInfoData.Name + userInfoData.FamilyName;
            var updateResult = await _userManager.UpdateAsync(userFromDb);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to update existing user information.");
            }

            await _signInManager.SignInAsync(userFromDb, isPersistent: false);
            var jwtToken = await _tokenService.GenerateTokenWithRefreshTokenAsync(userFromDb);

            var authResult = new AuthResult { Success = true, Token = jwtToken.AccessToken, Email = userFromDb.Email, ErrorMessage = "User authenticated successfully" };

            return authResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication callback.");
            return new AuthResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Refreshes the Google access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>A GoogleTokenResponse containing the refreshed token data.</returns>
    public async Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Refresh token cannot be null or empty.");
            }

            string tokenEndpoint = "https://oauth2.googleapis.com/token";
            string clientId = _configuration["Authentication:Google:ClientId"]!;
            string clientSecret = _configuration["Authentication:Google:ClientSecret"]!;

            var refreshRequestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            });

            HttpResponseMessage tokenResponse = await _httpClient.PostAsync(tokenEndpoint, refreshRequestContent);
            tokenResponse.EnsureSuccessStatusCode();

            string tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Refresh token response received: {TokenContent}", tokenContent);

            var tokenResponseData = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenContent);

            return tokenResponseData ?? throw new InvalidOperationException("Failed to deserialize token response.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Google access token.");
            throw;
        }
    }

    /// <summary>
    /// Registers a user externally using Google OAuth data.
    /// </summary>
    /// <param name="registerDto">The data transfer object containing registration data.</param>
    /// <returns>An ExternalRegisterDto containing the registration result.</returns>
    public async Task<ExternalRegisterDto> ExternalRegister(ExternalRegisterDto registerDto)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            if (registerDto == null || registerDto.UserInfo == null)
            {
                throw new ArgumentException("Registration data cannot be null.");
            }

            var newUser = new User
            {
                UserName = registerDto.UserInfo.Email,
                Email = registerDto.UserInfo.Email,
                EmailConfirmed = registerDto.UserInfo.ConfirmedEmail,
                IsDeleted = false,
                FullName = registerDto.UserInfo.Name + registerDto.UserInfo.FamilyName,
            };

            var createResult = await _userManager.CreateAsync(newUser);

            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException("Failed to create user.");
            }

            await _roleService.AssignRoleAsync(newUser.Id, _defaultRoles[0]);

            var userLogin = new IdentityUserLogin<string>
            {
                LoginProvider = registerDto.Provider,
                ProviderKey = registerDto.ProviderKey,
                ProviderDisplayName = registerDto.ProviderDisplayName,
                UserId = newUser.Id
            };

            var jwtToken = await _tokenService.GenerateTokenWithRefreshTokenAsync(newUser);

            var userToken = new IdentityUserToken<string>
            {
                UserId = newUser.Id,
                LoginProvider = registerDto.Provider,
                Name = "AccessToken",
                Value = jwtToken.AccessToken
            };

            _dbContext.UserTokens.Add(userToken);
            _dbContext.UserLogins.Add(userLogin);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var externalRegisterDto = new ExternalRegisterDto
            {
                Success = true,
                Token = jwtToken.AccessToken,
                Provider = registerDto.Provider,
                ProviderKey = registerDto.ProviderKey,
                ProviderDisplayName = registerDto.ProviderDisplayName,
                ErrorMessage = "User registered successfully.",
                UserInfo = new UserInfo()
                {
                    Id = newUser.Id,
                    Email = registerDto.UserInfo.Email,
                    Name = registerDto.UserInfo.Name,
                    FamilyName = registerDto.UserInfo.FamilyName,
                    ConfirmedEmail = registerDto.UserInfo.ConfirmedEmail
                }
            };

            return externalRegisterDto;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during external user registration.");
            return new ExternalRegisterDto { Success = false, ErrorMessage = "An unexpected error occurred." };
        }
    }
}
