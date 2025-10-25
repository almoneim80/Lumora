using Lumora.DTOs.Token;

namespace Lumora.Interfaces;

public interface IExternalAuthService
{
    /// <summary>
    /// Generates the Google OAuth 2.0 authentication URL.
    /// </summary>
    /// <returns>Authentication URL as a string.</returns>
    string GenerateGoogleLoginUrl(List<string> requestedScopes);

    /// <summary>
    /// Processes the authorization code returned by the OAuth provider.
    /// </summary>
    /// <param name="code">The authorization code received from the OAuth provider.</param>
    /// <returns>Information about the authenticated user.</returns>
    Task<AuthResult> HandleGoogleAuthCallbackAsync(string code);
}
