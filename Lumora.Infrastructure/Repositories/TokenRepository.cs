using Lumora.Application.Interfaces.TokenIntf;

namespace Lumora.Infrastructure.Repositories
{
    public class TokenRepository(PgDbContext dbContext) : ITokenRepository
    {
        public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string userId, string tokenHash, CancellationToken ct)
        {
            return await dbContext.RefreshTokens.FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.TokenHash == tokenHash &&
                !r.IsUsed &&
                !r.IsRevoked, ct);
        }

        public void UpdateRefreshToken(RefreshToken token)
        {
            dbContext.RefreshTokens.Update(token);
        }

        public async Task<RefreshToken?> GetValidRefreshTokenWithUserAsync(string hashedToken, CancellationToken ct)
        {
            return await dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt =>
                    rt.TokenHash == hashedToken &&
                    !rt.IsUsed &&
                    !rt.IsRevoked &&
                    rt.Expiration > DateTimeOffset.UtcNow, ct);
        }

        public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
        {
            await dbContext.RefreshTokens.AddAsync(token, ct);
        }
    }
}
