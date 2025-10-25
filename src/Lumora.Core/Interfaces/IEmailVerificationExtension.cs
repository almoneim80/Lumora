using Lumora.Configuration;

namespace Lumora.Interfaces;

public interface IEmailVerificationExtension
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task<string> GenerateConfirmationLink(User user, string token);
    Task<string> GenerateFallbackLinkAsync(User user);
    string GetKey(string id);
    string Generate(OtpVerificationOptions options, out DateTime expire, out string hash);
    bool Scan(string plain, string hash, OtpVerificationOptions options);
    string GenerateRandomString(int size);
    string ComputeHash(string plain, DateTime time, OtpVerificationOptions options);
    string Hash(string input, int length, int iterations);
    Task<string> GenerateResetPasswordLink(User user, string token);
}
