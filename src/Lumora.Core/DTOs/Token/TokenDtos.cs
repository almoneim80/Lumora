#nullable disable
namespace Lumora.DTOs.Token;

/// <summary>
/// DTO for JWT token.
/// </summary>
public class JWTokenDto
{
    [Required]
    required public string AccessToken { get; set; }

    [Required]
    required public DateTime AccessTokenExpiration { get; set; }
    [Required]
    required public string RefreshToken { get; set; }
}

/// <summary>
/// DTO for user information.
/// </summary>
public class UserInfo
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string FamilyName { get; set; }
    public bool ConfirmedEmail { get; set; }
}

/// <summary>
/// DTO for external authentication.
/// </summary>
public class ExternalRegisterDto
{
    public UserInfo UserInfo { get; set; }

    public string Provider { get; set; }
    public string ProviderKey { get; set; }
    public string ProviderDisplayName { get; set; }
    public string Token { get; set; }
    public string ErrorMessage { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// Result of a user authentication attempt.
/// </summary>
public class AuthResult
{
    public string ErrorMessage { get; set; }
    public bool Success { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
}

public class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; }
}

public class GoogleUserInfoResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("verified_email")]
    public bool VerifiedEmail { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("given_name")]
    public string GivenName { get; set; }

    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; }

    [JsonPropertyName("picture")]
    public string Picture { get; set; }

    [JsonPropertyName("locale")]
    public string Locale { get; set; }
}

public class TokenDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTimeOffset AccessTokenExpiration { get; set; }
#nullable enable
    public string? TokenType { get; set; } = "Bearer";
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public List<UserRoleDto>? Roles { get; set; }
}

public class UserRoleDto
{
#nullable disable
    public string Name { get; set; }
    public List<UserPermissionsDto> Permissions { get; set; }
}

public class UserPermissionsDto
{
    public string PermissionName { get; set; }
}
