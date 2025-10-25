using Lumora.Configuration;
namespace Lumora.Interfaces;

public interface IEmailVerificationService
{
    // Verification by link
    Task<(bool Succeeded, string Message, IEnumerable<IdentityError>? Errors)> ConfirmEmailAsync(string userId, string token);
    Task<(bool Succeeded, string Message)> ResendVerificationLinkAsync(string email);

    // Verification by code
    Task<(OtpVia Otp, DateTime Expire)> GenerateAsync(string id, OtpVerificationOptions options);
    Task<bool> VerifyOtpAsync(string id, string code);
    Task<(bool Succeeded, string? Message, DateTime? ExpireAt)> RegenerateOtpAsync(string userId);
}
