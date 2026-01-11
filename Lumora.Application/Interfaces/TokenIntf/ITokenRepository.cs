namespace Lumora.Application.Interfaces.TokenIntf
{
    public interface ITokenRepository
    {
        Task<RefreshToken?> GetActiveRefreshTokenAsync(string userId, string tokenHash, CancellationToken ct);
        void UpdateRefreshToken(RefreshToken token);
        Task<RefreshToken?> GetValidRefreshTokenWithUserAsync(string hashedToken, CancellationToken ct);
        Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    }
}
