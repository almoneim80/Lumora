using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Lumora.Configuration;

namespace Lumora.Services;

/// <summary>
/// Service for email verification, including OTP generation and verification.
/// </summary>
public class EmailVerificationService : ControllerBase, IEmailVerificationService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<EmailVerificationService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDataProtector _dataProtector;
    private readonly IOptions<OtpVerificationOptions> _options;
    private readonly IEmailVerificationExtension _emailVerificationExtension;

    public EmailVerificationService(
        UserManager<User> userManager,
        ILogger<EmailVerificationService> logger,
        IMemoryCache memoryCache,
        IOptions<OtpVerificationOptions> options,
        IDataProtectionProvider dataProtectionProvider,
        IEmailVerificationExtension emailVerificationExtension)
    {
        _userManager = userManager;
        _logger = logger;
        _memoryCache = memoryCache;
        _options = options;
        _dataProtector = dataProtectionProvider.CreateProtector("EmailVerificationService");
        _emailVerificationExtension = emailVerificationExtension;
    }

    // Verification by link

    /// <summary>
    /// Confirms a user's email using a token.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="token">The confirmation token.</param>
    /// <returns>A tuple indicating success, a message, and any errors.</returns>
    public async Task<(bool Succeeded, string Message, IEnumerable<IdentityError>? Errors)> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Invalid confirmation request. Missing userId or token.");
                return (false, "Invalid confirmation request.", null);
            }

            _logger.LogInformation("Confirming email for UserId: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with Id: {UserId}", userId);
                return (false, "User not found.", null);
            }

            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "EmailVerificationToken");
            if (storedToken == null)
            {
                _logger.LogWarning("No stored token found for UserId: {UserId}", userId);
                return (false, "Invalid token, please request a new one.", null);
            }

            if (!string.Equals(storedToken, token, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid token for UserId: {UserId}", userId);
                return (false, "Invalid token.", null);
            }

            var result = await _userManager.ConfirmEmailAsync(user, storedToken);
            if (result.Succeeded)
            {
                var removeTokenResult = await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "EmailVerificationToken");
                if (!removeTokenResult.Succeeded)
                {
                    _logger.LogWarning("Failed to remove confirmation token for UserId: {UserId}", userId);
                }

                _logger.LogInformation("Email confirmed successfully for UserId: {UserId}", userId);
                return (true, "Email confirmed successfully!", null);
            }
            else
            {
                var errorDetails = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Email confirmation failed for {Email}. Errors: {Errors}", user.Email, errorDetails);
                return (false, "Email confirmation failed.", result.Errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for UserId: {UserId}", userId);
            return (false, "An error occurred while confirming email.", null);
        }
    }

    /// <summary>
    /// Resends the email verification link to a user.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>A tuple indicating success and a message.</returns>
    public async Task<(bool Succeeded, string Message)> ResendVerificationLinkAsync(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email address is required for resending verification link.");
                return (false, "Email address is required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return (false, "User not found.");
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogInformation("Email already confirmed for user: {Email}", email);
                return (false, "Email is already confirmed.");
            }

            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "EmailVerificationToken");
            var token = storedToken ?? await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (storedToken == null)
            {
                await _userManager.SetAuthenticationTokenAsync(user, "Default", "EmailVerificationToken", token);
                _logger.LogInformation("New confirmation token generated and stored for user: {Email}", email);
            }
            else
            {
                _logger.LogInformation("Using existing confirmation token for user: {Email}", email);
            }

            var confirmationLink = await _emailVerificationExtension.GenerateConfirmationLink(user, token);

            if (!string.IsNullOrEmpty(confirmationLink))
            {
                await _emailVerificationExtension.SendEmailAsync(
                    user.Email!,
                    "Confirmation Email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>clicking here</a>.");
            }
            else
            {
                _logger.LogError("Failed to generate confirmation link for user: {UserId}", user.Id);
                await _emailVerificationExtension.GenerateFallbackLinkAsync(user);
                return (false, "Failed to send confirmation email. Please try again later or contact support.");
            }

            _logger.LogInformation("Confirmation email resent for user {UserId} to {Email}", user.Id, user.Email);
            return (true, "Confirmation email has been resent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification link for email: {Email}", email);
            return (false, "An error occurred while resending the verification email.");
        }
    }

    // Verification by code

    /// <summary>
    /// Generates an OTP for verification.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="options">OTP verification options.</param>
    /// <returns>A tuple containing the OTP and its expiration time.</returns>
    public Task<(OtpVia Otp, DateTime Expire)> GenerateAsync(string id, OtpVerificationOptions options)
    {
        try
        {
            var plain = _emailVerificationExtension.Generate(_options.Value, out DateTime expire, out string hash);

            _memoryCache.Set(_emailVerificationExtension.GetKey(id), hash, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expire,
                Priority = CacheItemPriority.High,
            });

            string url = _options.Value.EnableUrl && !string.IsNullOrWhiteSpace(_options.Value.BaseOtpUrl)
                ? _options.Value.BaseOtpUrl + _dataProtector.Protect(JsonSerializer.Serialize(new IdPlain(id, plain)))
                : string.Empty;

            return Task.FromResult((new OtpVia(plain, url), expire));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OTP for user ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Verifies an OTP for a user.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="code">The OTP code to verify.</param>
    /// <returns>True if the OTP is valid, otherwise false.</returns>
    public async Task<bool> VerifyOtpAsync(string id, string code)
    {
        try
        {
            var isValid = await VerifyAsync(id, code, _options.Value);

            if (isValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogError("User not found with ID: {Id}", id);
                    return false;
                }

                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    var updateResult = await _userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to update EmailConfirmed for user {Id}. Errors: {Errors}", id, errors);
                        return false;
                    }

                    _logger.LogInformation("EmailConfirmed updated successfully for user {Id}.", id);
                }

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for user ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Regenerates an OTP for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A tuple containing success status, message, and expiration time.</returns>
    public async Task<(bool Succeeded, string? Message, DateTime? ExpireAt)> RegenerateOtpAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return (false, "User not found.", null);
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("User {UserId} attempted to regenerate code, but email is already confirmed.", userId);
                return (false, "Email is already confirmed. No need to regenerate code.", null);
            }

            _memoryCache.Remove(_emailVerificationExtension.GetKey(userId));
            var (otp, expire) = await GenerateAsync(userId, _options.Value);

            await _emailVerificationExtension.SendEmailAsync(
                    user.Email!,
                    "Verification Code",
                    $"Your verification code is: <b>{otp.Plain}<b>. It will expire after {expire} minutes.");

            _logger.LogInformation("New verification code has been sent. Please check your email. Your code will expire in {Expire} minutes.", _options.Value.Expire);

            return (true, "New code generated and sent.", expire);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating OTP for user ID: {UserId}", userId);
            return (false, "An error occurred while regenerating the code.", null);
        }
    }

    /// <summary>
    /// Verifies an OTP code for a user.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="code">The OTP code to verify.</param>
    /// <param name="options">OTP verification options.</param>
    /// <returns>True if the OTP is valid, otherwise false.</returns>
    public async Task<bool> VerifyAsync(string id, string code, OtpVerificationOptions options)
    {
        try
        {
            string? hash = await Task.FromResult(_memoryCache.Get<string>(_emailVerificationExtension.GetKey(id)));

            if (string.IsNullOrEmpty(hash))
            {
                _logger.LogWarning("No OTP hash found for user ID: {Id}", id);
                return false;
            }

            if (_emailVerificationExtension.Scan(code, hash, _options.Value))
            {
                _memoryCache.Remove(_emailVerificationExtension.GetKey(id));
                _logger.LogInformation("OTP verified successfully for user ID: {Id}", id);
                return true;
            }

            _logger.LogWarning("OTP verification failed for user ID: {Id}", id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for user ID: {Id}", id);
            throw;
        }
    }
}
