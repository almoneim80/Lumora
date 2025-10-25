using Lumora.DTOs.Token;

namespace Lumora.Interfaces.Token
{
    public interface ITokenService
    {
        /// <summary>
        /// Generate JWT token with refresh token.
        /// </summary>
        Task<JWTokenDto> GenerateTokenWithRefreshTokenAsync(User user, IEnumerable<Claim>? extraClaims = null, TimeSpan? expiresIn = null);

        /// <summary>
        /// Generate refresh token.
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Hash refresh token.
        /// </summary>
        string HashRefreshToken(string rawToken);
    }
}
