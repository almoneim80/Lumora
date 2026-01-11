namespace Lumora.Application.Interfaces.ClubIntf
{
    public interface IWheelRepository
    {
        // Player Plays
        Task<int> GetPlayerSpinCountAsync(string playerId, DateTimeOffset start, DateTimeOffset end);
        Task AddWheelPlayerAsync(WheelPlayer entry);
        Task<WheelPlayer?> GetWheelPlayerByIdAsync(int playId);

        // Player State
        Task<WheelPlayerState?> GetPlayerStateAsync(string playerId, DateTimeOffset date);
        Task AddPlayerStateAsync(WheelPlayerState state);

        // Awards
        Task<WheelAward?> GetAwardByIdAsync(int awardId);

        // Query for Pagination (Physical Items)
        IQueryable<WheelPlayer> GetPhysicalItemsQuery(bool? isDelivered);

        // Payments (Specific to wheel needs)
        Task<int> GetPaidRetriesCountAsync(string playerId, DateTimeOffset start, DateTimeOffset end);

        // State Management
        Task UpdatePlayerStateAsync(WheelPlayerState state);

        // Security Checks
        Task<List<string>> GetPlayerIdsByConnectionDetailsAsync(string ip, string device);
        Task<IEnumerable<object>> GetPlayerHistorySimpleAsync(string playerId);
        Task<IEnumerable<TResult>> GetPlayerSpinsInDateRangeAsync<TResult>(
            string playerId, DateTimeOffset start, DateTimeOffset end, Expression<Func<WheelPlayer, TResult>> selector);
        Task UpdateWheelPlayerAsync(WheelPlayer entry);
        IQueryable<WheelPlayer> GetAllPlaysWithDetailsQuery();
        IQueryable<WheelPlayDto> GetPhysicalItemPlaysProjected(bool? isDelivered);
        void UpdateWheelPlayer(WheelPlayer entry);
        Task<PagedResult<WheelPlayDto>> GetPagedPhysicalItemPlaysAsync(
                bool? isDelivered, int pageNumber, int pageSize, CancellationToken ct);

        Task<PagedResult<WheelPlayDto>> GetAllUserPlaysPagedAsync(
            int pageNumber, int pageSize, CancellationToken ct);

        Task<PagedResult<WheelPlayDto>> GetPlaysByDeliveryStatusPagedAsync(
            bool delivered, int pageNumber, int pageSize, CancellationToken ct);
    }
}
