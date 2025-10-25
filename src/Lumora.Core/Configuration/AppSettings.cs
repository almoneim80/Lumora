namespace Lumora.Configuration;

public class EntitiesConfig
{
    public string[] Include { get; set; } = Array.Empty<string>();

    public string[] Exclude { get; set; } = Array.Empty<string>();
}

public class MethodsReturnData
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? AdditionalData { get; set; }
}

// CacheSettings 
public class CacheSettings
{
    public int CacheExpirationMinutes { get; set; }
    public CacheItemPriority CacheItemPriority { get; set; } = CacheItemPriority.Normal;
    public long? CacheItemSize { get; set; }
}

// EmailSenderOptions
public class EmailSenderOptions
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

// Localization 
public class LocalizationSettings
{
    public string? DefaultCulture { get; set; }
    public string? RatingsPath { get; set; }
    public string? CulturesPath { get; set; }
    public string? CountriesPath { get; set; }
}

// Otp Verification Options 
public class OtpVerificationOptions
{
    // Enable or disable in-memory cache
    public bool IsInMemoryCache { get; set; }

    // Active to generate URL to verify code with Id OTP
    public bool EnableUrl { get; set; }
    public int Iterations { get; set; }
    public int Size { get; set; }
    public int Length { get; set; }
    public int Expire { get; set; }
    public string? BaseOtpUrl { get; set; }
}

// Enum Data
public class EnumData
{
    public int Value { get; set; }
    public string? Description { get; set; }
}

public class BaseServiceConfig
{
    public string Server { get; set; } = string.Empty;

    public int Port { get; set; } = 0;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

// Result DTO for create user response 
public class ResultDto
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}

public class PostgresConfig : BaseServiceConfig
{
    public string Database { get; set; } = string.Empty;

    public string ConnectionString => $"User ID={UserName};Password={Password};Server={Server};Port={Port};Database={Database};Pooling=true;";
}

public class ElasticConfig : BaseServiceConfig
{
    public bool UseHttps { get; set; } = false;

    public string IndexPrefix { get; set; } = string.Empty;

    public string Url => $"http{(UseHttps ? "s" : string.Empty)}://{Server}:{Port}";
}

public class ExtensionConfig
{
    public string Extension { get; set; } = string.Empty;

    public string MaxSize { get; set; } = string.Empty;
}

public class MediaConfig
{
    public string[] Extensions { get; set; } = Array.Empty<string>();
    public ExtensionConfig[] MaxSize { get; set; } = Array.Empty<ExtensionConfig>();
    public string? CacheTime { get; set; }
}

public class FileConfig
{
    public string[] Extensions { get; set; } = Array.Empty<string>();

    public ExtensionConfig[] MaxSize { get; set; } = Array.Empty<ExtensionConfig>();
}

public class EmailVerificationApiConfig
{
    public string Url { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

public class AccountDetailsApiConfig
{
    public string Url { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

public class ApiSettingsConfig
{
    public int MaxListSize { get; set; }

    public string MaxImportSize { get; set; } = string.Empty;

    public string DefaultLanguage { get; set; } = "en-US";
}

public class GeolocationApiConfig
{
    public string Url { get; set; } = string.Empty;

    public string AuthKey { get; set; } = string.Empty;
}

public class TaskConfig
{
    public bool Enable { get; set; }

    public string CronSchedule { get; set; } = string.Empty;

    public int RetryCount { get; set; }

    public int RetryInterval { get; set; }
}

public class TaskWithBatchConfig : TaskConfig
{
    public int BatchSize { get; set; }
}

public class DomainVerificationTaskConfig : TaskWithBatchConfig
{
    public int BatchInterval { get; set; }
}

public class CacheProfileSettings
{
    public string Type { get; set; } = string.Empty;

    public string VaryByHeader { get; set; } = string.Empty;

    public int? DurationInDays { get; set; }
}

public class AppSettings
{
    public PostgresConfig Postgres { get; set; } = new PostgresConfig();

    public ElasticConfig Elastic { get; set; } = new ElasticConfig();
}

public class CorsConfig
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

public class EmailConfig : BaseServiceConfig
{
    public bool UseSsl { get; set; }
}

public class JwtConfig
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public int RefreshTokenExpirationDays { get; set; } = 30;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
}

public class UpdateRoleDto
{
    public string OldRoleName { get; set; } = string.Empty;
    public string NewRoleName { get; set; } = string.Empty;
}

public class DefaultRolesConfig : List<string>
{
}

public class DefaultUserConfig
{
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool PhoneNumberConfirmed { get; set; } = true;
    public bool EmailConfirmed { get; set; } = true;

    public DefaultRolesConfig Roles { get; set; } = new DefaultRolesConfig();
}

public class DefaultUsersConfig : List<DefaultUserConfig>
{
}

public class AssignRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UserInRole
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AssignRoleByEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class CookiesConfig
{
    public bool Enable { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ExpireTime { get; set; } = 12; // Gets or sets expiration time in hours.
}

public class IdentityConfig
{
    public double LockoutTime { get; set; } = 15;

    public int MaxFailedAccessAttempts { get; set; } = 5;
}

public class AffiliateSettings
{
    public decimal MinimumPayoutAmount { get; set; }
}

#nullable disable
public class FileUploadSettings
{
    public FileCategorySetting Images { get; set; }
    public FileCategorySetting Videos { get; set; }
    public FileCategorySetting Documents { get; set; }
}

public class FileCategorySetting
{
    public List<string> Extensions { get; set; }
    public List<string> MimeTypes { get; set; }
    public Dictionary<string, string> MaxSizePerExtension { get; set; }
}
public class FfmpegSettings
{
    public string ExecutablesPath { get; set; } = string.Empty;
}
