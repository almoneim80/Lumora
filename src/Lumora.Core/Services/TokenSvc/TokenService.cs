using Lumora.DTOs.Token;

namespace Lumora.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TokenService> _logger;
        public TokenService(IOptions<JwtConfig> jwtConfig, UserManager<User> userManager, ILogger<TokenService> logger)
        {
            _jwtConfig = jwtConfig.Value;
            _userManager = userManager;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<JWTokenDto> GenerateTokenWithRefreshTokenAsync(User user, IEnumerable<Claim>? extraClaims = null, TimeSpan? expiresIn = null)
        {
            try
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

                var roles = await _userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                if (extraClaims != null)
                {
                    claims.AddRange(extraClaims);
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
                var expires = DateTime.UtcNow.Add(expiresIn ?? TimeSpan.FromHours(_jwtConfig.AccessTokenExpirationMinutes));

                var token = new JwtSecurityToken(
                    issuer: _jwtConfig.Issuer,
                    audience: _jwtConfig.Audience,
                    expires: expires,
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                var refreshToken = GenerateRefreshToken();

                return new JWTokenDto
                {
                    AccessToken = tokenString,
                    AccessTokenExpiration = expires,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate token with refresh token.");
                throw;
            }
        }

        /// <inheritdoc/>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <inheritdoc/>
        public string HashRefreshToken(string rawToken)
        {
            if (string.IsNullOrEmpty(rawToken))
            {
                return string.Empty;
            }

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
