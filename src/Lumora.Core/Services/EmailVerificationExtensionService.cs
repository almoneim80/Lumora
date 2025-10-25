using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Lumora.Configuration;

namespace Lumora.Services;

/// <summary>
/// Service extension for email verification, including OTP generation and link-based verification.
/// </summary>
public class EmailVerificationExtensionService : IEmailVerificationExtension
{
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailVerificationExtensionService> _logger;
    private readonly UserManager<User> _userManager;

    public EmailVerificationExtensionService(
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<EmailVerificationExtensionService> logger,
        UserManager<User> userManager)
    {
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
    }

    // // Verification by code

    /// <summary>
    /// Generates an OTP and its hash for verification.
    /// </summary>
    /// <param name="options">The OTP verification options.</param>
    /// <param name="expire">The expiration time of the OTP.</param>
    /// <param name="hash">The hash of the OTP.</param>
    /// <returns>The generated OTP.</returns>
    public string Generate(OtpVerificationOptions options, out DateTime expire, out string hash)
    {
        try
        {
            var utcNow = DateTime.UtcNow;
            var plain = GenerateRandomString(options.Size);
            expire = utcNow.AddMinutes(options.Expire);
            hash = ComputeHash(plain, utcNow, options);
            _logger.LogInformation("OTP generated successfully.");
            return plain;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OTP.");
            throw;
        }
    }

    /// <summary>
    /// Scans an OTP against its hash to verify its validity.
    /// </summary>
    /// <param name="plain">The plain OTP.</param>
    /// <param name="hash">The hashed OTP.</param>
    /// <param name="options">The OTP verification options.</param>
    /// <returns>True if the OTP is valid, otherwise false.</returns>
    public bool Scan(string plain, string hash, OtpVerificationOptions options)
    {
        try
        {
            var utcNow = DateTime.UtcNow;

            for (int minuteOffset = 0; minuteOffset <= options.Expire; minuteOffset++)
            {
                var checkTime = utcNow.AddMinutes(-minuteOffset);
                var hashToCheck = ComputeHash(plain, checkTime, options);

                if (hash.Equals(hashToCheck, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("OTP validated successfully.");
                    return true;
                }
            }

            _logger.LogWarning("OTP validation failed.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning OTP.");
            throw;
        }
    }

    /// <summary>
    /// Generates a random OTP string of specified size.
    /// </summary>
    /// <param name="size">The size of the OTP.</param>
    /// <returns>The generated OTP.</returns>
    public string GenerateRandomString(int size)
    {
        try
        {
            const string digits = "0123456789";
            var otp = new string(Enumerable.Repeat(digits, size)
                .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
            _logger.LogInformation("Random OTP string generated.");
            return otp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating random OTP string.");
            throw;
        }
    }

    /// <summary>
    /// Computes the hash of an OTP using the specified options.
    /// </summary>
    /// <param name="plain">The plain OTP.</param>
    /// <param name="time">The time used for hashing.</param>
    /// <param name="options">The OTP verification options.</param>
    /// <returns>The computed hash.</returns>
    public string ComputeHash(string plain, DateTime time, OtpVerificationOptions options)
    {
        try
        {
            var input = plain + time.ToString("yyyyMMddHHmm");
            var hash = Hash(input, options.Length, options.Iterations);
            _logger.LogInformation("Hash computed successfully.");
            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing hash.");
            throw;
        }
    }

    /// <summary>
    /// Generates a secure hash for the input string using SHA256.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="length">The desired length of the hash.</param>
    /// <param name="iterations">The number of hashing iterations.</param>
    /// <returns>The generated hash.</returns>
    public string Hash(string input, int length, int iterations)
    {
        try
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            for (int i = 0; i < iterations; i++)
            {
                bytes = sha256.ComputeHash(bytes);
            }

            var result = BitConverter.ToString(bytes).Replace("-", "").Substring(0, length).ToLowerInvariant();
            _logger.LogInformation("Hash generated successfully.");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hash.");
            throw;
        }
    }

    /// <summary>
    /// Generates a cache key for OTP storage.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The generated cache key.</returns>
    public string GetKey(string id)
    {
        try
        {
            var key = $"otp:{id}";
            _logger.LogInformation("Cache key generated: {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cache key.");
            throw;
        }
    }

    // Verification by link

    /// <summary>
    /// Sends an email with the specified details.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body of the email.</param>
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var fromEmail = _configuration["EmailSender:FromEmail"];
            var fromName = _configuration["EmailSender:FromName"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName))
            {
                throw new InvalidOperationException("Email settings (FromEmail or FromName) are not configured in appsettings.json.");
            }

            await _emailService.SendAsync(subject, fromEmail, fromName, new[] { toEmail }, body, null);

            _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}.", toEmail);
            throw;
        }
    }

    /// <summary>
    /// Generates an email confirmation link.
    /// </summary>
    /// <param name="user">The user for whom the link is generated.</param>
    /// <param name="token">The confirmation token.</param>
    /// <returns>The generated confirmation link.</returns>
    public async Task<string> GenerateConfirmationLink(User user, string token)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available");
            }

            var request = httpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var confirmationLink = $"{baseUrl}/api/Email/ConfirmEmail?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";

            _logger.LogInformation("Generated confirmation link for user {UserId}: {ConfirmationLink}", user.Id, confirmationLink);
            return await Task.FromResult(confirmationLink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating confirmation link for user {UserId}.", user?.Id);
            throw;
        }
    }

    /// <summary>
    /// Generates a reset password link.
    /// </summary>
    /// <param name="user">The user for whom the link is generated.</param>
    /// <param name="token">The reset token.</param>
    /// <returns>The generated reset password link.</returns>
    public async Task<string> GenerateResetPasswordLink(User user, string token)
    {
        try
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("BaseUrl is not configured.");
            }

            var resetPasswordLink = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
            _logger.LogInformation("Generated reset password link for user {UserId}: {ResetPasswordLink}", user.Id, resetPasswordLink);
            return await Task.FromResult(resetPasswordLink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating reset password link for user {UserId}.", user?.Id);
            throw;
        }
    }

    /// <summary>
    /// Generates a fallback confirmation link if primary link generation fails.
    /// </summary>
    /// <param name="user">The user for whom the link is generated.</param>
    /// <returns>The generated fallback link.</returns>
    public async Task<string> GenerateFallbackLinkAsync(User user)
    {
        try
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is unavailable. Ensure this method is called in an HTTP context.");
            }

            string baseUrl = _configuration["AppSettings:BaseUrl"] ?? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            string fallbackToken = await _userManager.GenerateUserTokenAsync(user, "Default", "FallbackConfirmation");

            string fallbackLink = $"{baseUrl}/Account/ConfirmEmail?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(fallbackToken)}";

            _logger.LogInformation("Generated fallback link for user {UserId}: {FallbackLink}", user.Id, fallbackLink);

            var emailBody = $"""
                We encountered an issue generating the primary confirmation link. 
                Please confirm your account by <a href='{HtmlEncoder.Default.Encode(fallbackLink)}'>clicking here</a>.
                If this issue persists, contact support for assistance. Thank you.
                """;

            await SendEmailAsync(
                user.Email!,
                "Confirm Your Email - Fallback Link",
                emailBody);

            return fallbackLink;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating fallback link for user {UserId}.", user?.Id);
            throw;
        }
    }
}
