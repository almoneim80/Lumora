using Lumora.Application.DTOs.Club;
using Lumora.Application.Interfaces.ClubIntf;
using System.Linq.Expressions;

namespace Lumora.Infrastructure.Repositories
{
    public class WheelRepository(PgDbContext dbContext) : IWheelRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<int> GetPlayerSpinCountAsync(string playerId, DateTimeOffset start, DateTimeOffset end)
        {
            return await _dbContext.WheelPlayers
                .CountAsync(x => x.PlayerId == playerId && x.PlayedAt >= start && x.PlayedAt < end);
        }

        public async Task<int> GetPaidRetriesCountAsync(string playerId, DateTimeOffset start, DateTimeOffset end)
        {
            return await _dbContext.PaymentItems
                .Include(pi => pi.Payment)
                .CountAsync(pi =>
                    pi.Payment.UserId == playerId &&
                    pi.ItemType == PaymentItemType.SpinWheel &&
                    !pi.Payment.IsDeleted &&
                    pi.Payment.Status == PaymentStatus.Paid &&
                    pi.Payment.CreatedAt >= start &&
                    pi.Payment.CreatedAt < end);
        }

        public async Task<WheelPlayerState?> GetPlayerStateAsync(string playerId, DateTimeOffset date)
        {
            return await _dbContext.WheelPlayerStates
                .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.Date == date);
        }

        public async Task AddPlayerStateAsync(WheelPlayerState state)
        {
            await _dbContext.WheelPlayerStates.AddAsync(state);
        }

        public async Task<WheelAward?> GetAwardByIdAsync(int awardId)
        {
            return await _dbContext.WheelAwards
                .FirstOrDefaultAsync(x => x.Id == awardId && !x.IsDeleted);
        }

        public async Task AddWheelPlayerAsync(WheelPlayer entry)
        {
            await _dbContext.WheelPlayers.AddAsync(entry);
        }

        public async Task<WheelPlayer?> GetWheelPlayerByIdAsync(int playId)
        {
            return await _dbContext.WheelPlayers
                .Include(x => x.Award)
                .FirstOrDefaultAsync(x => x.Id == playId);
        }

        public IQueryable<WheelPlayer> GetPhysicalItemsQuery(bool? isDelivered)
        {
            var query = _dbContext.WheelPlayers
                .Include(x => x.Award)
                .Include(x => x.Player)
                .Where(x => !x.IsDeleted && x.Award.Type == AwardType.PhysicalItem);

            if (isDelivered.HasValue)
                query = query.Where(x => x.IsDelivered == isDelivered.Value);

            return query.OrderByDescending(x => x.PlayedAt);
        }

        public async Task UpdatePlayerStateAsync(WheelPlayerState state)
        {
            _dbContext.WheelPlayerStates.Update(state);
            await Task.CompletedTask;
        }

        public async Task<List<string>> GetPlayerIdsByConnectionDetailsAsync(string ip, string device)
        {
            return await _dbContext.WheelPlayers
                .Where(x => x.IpAddress == ip || x.DeviceInfo == device)
                .Select(x => x.PlayerId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetPlayerHistorySimpleAsync(string playerId)
        {
            return await _dbContext.WheelPlayers
                .Where(x => x.PlayerId == playerId && !x.IsDeleted)
                .Include(x => x.Award)
                .OrderByDescending(x => x.PlayedAt)
                .Select(x => new
                {
                    x.AwardId,
                    AwardName = x.Award.Name,
                    x.PlayedAt,
                    x.IsFree,
                    x.DeviceInfo,
                    x.IpAddress
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetPlayerSpinsInDateRangeAsync<TResult>(
            string playerId, DateTimeOffset start, DateTimeOffset end, Expression<Func<WheelPlayer, TResult>> selector)
        {
            return await _dbContext.WheelPlayers
                .AsNoTracking()
                .Include(x => x.Award)
                .Where(x =>
                    x.PlayerId == playerId &&
                    x.PlayedAt >= start &&
                    x.PlayedAt < end &&
                    !x.IsDeleted)
                .Select(selector)
                .ToListAsync();
        }

        public async Task UpdateWheelPlayerAsync(WheelPlayer entry)
        {
            _dbContext.WheelPlayers.Update(entry);
            await Task.CompletedTask;
        }
        public IQueryable<WheelPlayer> GetAllPlaysWithDetailsQuery()
        {
            return _dbContext.WheelPlayers
                .Include(x => x.Award)
                .Include(x => x.Player)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.PlayedAt);
        }

        public IQueryable<WheelPlayDto> GetPhysicalItemPlaysProjected(bool? isDelivered)
        {
            var query = _dbContext.WheelPlayers
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Award.Type == AwardType.PhysicalItem);

            if (isDelivered.HasValue)
                query = query.Where(x => x.IsDelivered == isDelivered.Value);

            return query
                .OrderByDescending(x => x.PlayedAt)
                .Select(x => new WheelPlayDto
                {
                    Id = x.Id,
                    AwardName = x.Award.Name,
                    PlayedAt = x.PlayedAt,
                    IsFree = x.IsFree,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    PlayerData = new PlayerData
                    {
                        FullName = x.Player.FullName,
                        Email = x.Player.Email
                    }
                });
        }
        public void UpdateWheelPlayer(WheelPlayer entry)
        {
            _dbContext.WheelPlayers.Update(entry);
        }

        public async Task<PagedResult<WheelPlayDto>> GetPagedPhysicalItemPlaysAsync(
                    bool? isDelivered, int pageNumber, int pageSize, CancellationToken ct)
        {
            var query = _dbContext.WheelPlayers
                .AsNoTracking()
                .Where(x => x.Award.Type == AwardType.PhysicalItem);

            if (isDelivered.HasValue)
            {
                query = query.Where(x => x.IsDelivered == isDelivered.Value);
            }

            // استخدام الـ Extension الذي قمت بتعريفه سابقاً
            return await query.Select(x => new WheelPlayDto
            {
                Id = x.Id,
                AwardName = x.Award.Name,
                PlayedAt = x.PlayedAt,
                IsFree = x.IsFree,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                PlayerData = new PlayerData
                {
                    FullName = x.Player.FullName,
                    Email = x.Player.Email
                }
            }).ApplyPaginationAsync(new PaginationRequestDto { PageNumber = pageNumber, PageSize = pageSize }, ct);
        }

        public async Task<PagedResult<WheelPlayDto>> GetAllUserPlaysPagedAsync(
            int pageNumber, int pageSize, CancellationToken ct)
        {
            return await _dbContext.WheelPlayers
                .AsNoTracking()
                .Select(x => new WheelPlayDto
                {
                    Id = x.Id,
                    AwardName = x.Award.Name,
                    PlayedAt = x.PlayedAt,
                    IsFree = x.IsFree,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    PlayerData = new PlayerData
                    {
                        FullName = x.Player.FullName,
                        Email = x.Player.Email
                    }
                })
                .ApplyPaginationAsync(new PaginationRequestDto { PageNumber = pageNumber, PageSize = pageSize }, ct);
        }

        public async Task<PagedResult<WheelPlayDto>> GetPlaysByDeliveryStatusPagedAsync(
            bool delivered, int pageNumber, int pageSize, CancellationToken ct)
        {
            return await _dbContext.WheelPlayers
                .AsNoTracking()
                .Where(x => x.Award.Type == AwardType.PhysicalItem && x.IsDelivered == delivered)
                .Select(x => new WheelPlayDto
                {
                    Id = x.Id,
                    AwardName = x.Award.Name,
                    PlayedAt = x.PlayedAt,
                    IsFree = x.IsFree,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    PlayerData = new PlayerData
                    {
                        FullName = x.Player.FullName,
                        Email = x.Player.Email
                    }
                })
                .ApplyPaginationAsync(new PaginationRequestDto { PageNumber = pageNumber, PageSize = pageSize }, ct);
        }
    }
}
