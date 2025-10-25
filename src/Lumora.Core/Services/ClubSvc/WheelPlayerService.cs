using Microsoft.Extensions.Configuration.UserSecrets;
using Lumora.DTOs.Club;
using Lumora.Extensions;
using Lumora.Interfaces.ClubIntf;
using Lumora.Interfaces.PaymentIntf;
namespace Lumora.Services.ClubIntf
{
    public class WheelPlayerService(
        PgDbContext dbContext,
        ILocalizationManager localization,
        ILogger<WheelPlayerService> logger,
        IPaymentRepository paymentRepository,
        IHttpContextHelper httpContextHelper) : IWheelPlayerService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILocalizationManager _localization = localization;
        private readonly ILogger<WheelPlayerService> _logger = logger;
        private readonly IPaymentRepository _ = paymentRepository;

        /// <inheritdoc/>
        public async Task<GeneralResult> CanPlayTodayAsync(string playerId)
        {
            try
            {
                var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
                var tomorrowStart = todayStart.AddDays(1);

                var todayPlays = await _dbContext.WheelPlayers
                    .Where(x => x.PlayerId == playerId &&
                                x.PlayedAt >= todayStart &&
                                x.PlayedAt < tomorrowStart)
                    .CountAsync();

                var paidRetries = await _dbContext.PaymentItems
                    .Include(pi => pi.Payment)
                    .Where(pi =>
                        pi.Payment.UserId == playerId &&
                        pi.ItemType == PaymentItemType.SpinWheel &&
                        !pi.Payment.IsDeleted &&
                        pi.Payment.Status == PaymentStatus.Paid &&
                        pi.Payment.CreatedAt >= todayStart &&
                        pi.Payment.CreatedAt < tomorrowStart)
                    .CountAsync();

                var canPlay = todayPlays == 0 || (todayPlays - 1) < paidRetries;

                return new GeneralResult
                {
                    IsSuccess = canPlay,
                    Message = canPlay
                        ? _localization.GetLocalizedString("CanPlayToday")
                        : _localization.GetLocalizedString("NoMorePlaysAllowed"),
                    Data = canPlay
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking play permission for user {PlayerId}", playerId);
                return new GeneralResult
                {
                    IsSuccess = false,
                    Message = _localization.GetLocalizedString("PlayCheckFailed"),
                    Data = null
                };
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SpinAsync(string playerId, int awardId)
        {
            try
            {
                var today = DateTimeOffset.UtcNow.Date.ToUniversalTime();

                // حاول جلب الحالة، وإن لم توجد أنشئها على الفور
                var state = await _dbContext.WheelPlayerStates
                    .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.Date == today);

                if (state == null)
                {
                    state = new WheelPlayerState
                    {
                        PlayerId = playerId,
                        Date = today,
                        HasUsedFreeSpin = false,
                        AllowPaidSpin = false
                    };

                    _dbContext.WheelPlayerStates.Add(state);
                    await _dbContext.SaveChangesAsync();
                }

                // تحقق من صلاحية اللعب
                if (state.HasUsedFreeSpin && !state.AllowPaidSpin)
                {
                    return new GeneralResult
                    {
                        IsSuccess = false,
                        Message = _localization.GetLocalizedString("AlreadyPlayedToday"),
                        Data = null
                    };
                }

                // تحقق من الجهاز وIP للتنبيه إن وجد تكرار
                var device = httpContextHelper.UserAgent;
                var ip = httpContextHelper.IpAddressV4;

                var duplicateAccounts = await _dbContext.WheelPlayers
                    .Where(x => x.IpAddress == ip || x.DeviceInfo == device)
                    .Select(x => x.PlayerId)
                    .Distinct()
                    .ToListAsync();

                if (duplicateAccounts.Count > 1)
                {
                    _logger.LogWarning("Suspicious spin: multiple accounts using same device/IP. PlayerId: {PlayerId}, IP: {Ip}, Device: {Device}", playerId, ip, device);
                }

                // تحقق من وجود الجائزة
                var existsAward = await _dbContext.WheelAwards.FirstOrDefaultAsync(x => x.Id == awardId && !x.IsDeleted);
                if (existsAward == null)
                {
                    return new GeneralResult
                    {
                        IsSuccess = false,
                        Message = _localization.GetLocalizedString("NoAwardsAvailable"),
                        Data = null
                    };
                }

                var isFree = !state.HasUsedFreeSpin;

                var entry = new WheelPlayer
                {
                    PlayerId = playerId,
                    AwardId = awardId,
                    PlayedAt = DateTimeOffset.UtcNow,
                    IsFree = isFree,
                    DeviceInfo = device,
                    IpAddress = ip
                };

                _dbContext.WheelPlayers.Add(entry);

                if (isFree)
                    state.HasUsedFreeSpin = true;
                else
                    state.AllowPaidSpin = false;

                await _dbContext.SaveChangesAsync();

                return new GeneralResult
                {
                    IsSuccess = true,
                    Message = _localization.GetLocalizedString("SpinSuccess"),
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Spin error for user {PlayerId}", playerId);
                return new GeneralResult
                {
                    IsSuccess = false,
                    Message = _localization.GetLocalizedString("SpinFailed"),
                    Data = null
                };
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> GetPlayerHistoryAsync(string playerId)
        {
            try
            {
                var history = await _dbContext.WheelPlayers
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
                    }).ToListAsync();

                return new GeneralResult
                {
                    IsSuccess = true,
                    Message = _localization.GetLocalizedString("PlayerHistoryLoaded"),
                    Data = history
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching spin history for {PlayerId}", playerId);
                return new GeneralResult
                {
                    IsSuccess = false,
                    Message = _localization.GetLocalizedString("PlayerHistoryFailed"),
                    Data = null
                };
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> GetTodaySpinAsync(string playerId)
        {
            try
            {
                var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
                var tomorrow = today.AddDays(1);

                var spins = await _dbContext.WheelPlayers
                    .Include(x => x.Award)
                    .Where(x =>
                        x.PlayerId == playerId &&
                        x.PlayedAt >= today &&
                        x.PlayedAt < tomorrow &&
                        !x.IsDeleted)
                    .Select(x => new
                    {
                        x.AwardId,
                        AwardName = x.Award.Name,
                        x.PlayedAt,
                        x.IsFree,
                        x.DeviceInfo,
                        x.IpAddress
                    }).ToListAsync();

                if (!spins.Any())
                {
                    return new GeneralResult
                    {
                        IsSuccess = false,
                        Message = _localization.GetLocalizedString("NoSpinToday"),
                        Data = null
                    };
                }

                return new GeneralResult
                {
                    IsSuccess = true,
                    Message = _localization.GetLocalizedString("TodaySpinLoaded"),
                    Data = spins
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching today's spin for {PlayerId}", playerId);
                return new GeneralResult
                {
                    IsSuccess = false,
                    Message = _localization.GetLocalizedString("TodaySpinFailed"),
                    Data = null
                };
            }
        }

        /// <inheritdoc/>
        public async Task EnsurePlayerSpinStateAsync(string playerId)
        {
            var today = DateTimeOffset.UtcNow;

            var state = await _dbContext.WheelPlayerStates
                .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.Date == today);

            if (state == null)
            {
                _dbContext.WheelPlayerStates.Add(new WheelPlayerState
                {
                    PlayerId = playerId,
                    Date = today,
                    HasUsedFreeSpin = false,
                    AllowPaidSpin = false
                });

                await _dbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task ActivatePaidSpinAsync(string playerId)
        {
            var today = DateTimeOffset.UtcNow.Date;
            var state = await _dbContext.WheelPlayerStates
                .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.Date == today);

            if (state != null)
            {
                state.AllowPaidSpin = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> MarkPlayDeliveredAsync(int playId, bool isDelivered)
        {
            var play = await _dbContext.WheelPlayers
                .Include(x => x.Award)
                .FirstOrDefaultAsync(x => x.Id == playId);

            if (play == null)
            {
                _logger.LogWarning("MarkPlayDeliveredAsync: Play not found. ID={PlayId}", playId);
                return new GeneralResult(false, _localization.GetLocalizedString("PlayNotFound"));
            }

            if (play.Award.Type != AwardType.PhysicalItem)
            {
                _logger.LogWarning("MarkPlayDeliveredAsync: Attempted to mark non-physical award as delivered. ID={PlayId}", playId);
                return new GeneralResult(false, _localization.GetLocalizedString("InvalidAwardType"));
            }

            if (play.IsDelivered == isDelivered)
            {
                return new GeneralResult(true, _localization.GetLocalizedString("PlayStatusUnchanged"));
            }

            play.IsDelivered = isDelivered;
            play.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("MarkPlayDeliveredAsync: Updated delivery status for PlayId={PlayId} to {Status}", playId, isDelivered);

            return new GeneralResult(true, _localization.GetLocalizedString("PlayStatusUpdated"));
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<WheelPlayDto>>> GetPlaysByDeliveryStatusAsync(
            bool delivered, PaginationRequestDto pagination, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.WheelPlayers
                .Include(x => x.Award)
                .Include(x => x.Player)
                .Where(x => !x.IsDeleted && x.IsDelivered == delivered && x.Award.Type == AwardType.PhysicalItem)
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

            var pagedResult = await query.ApplyPaginationAsync(pagination, cancellationToken);

            return new GeneralResult<PagedResult<WheelPlayDto>>(
                true,
                _localization.GetLocalizedString("PlaysByDeliveryLoaded"),
                pagedResult);
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<WheelPlayDto>>> GetAllUserPlaysAsync(
            PaginationRequestDto pagination, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.WheelPlayers
                .Include(x => x.Award)
                .Include(x => x.Player)
                .Where(x => !x.IsDeleted)
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

            var pagedResult = await query.ApplyPaginationAsync(pagination, cancellationToken);

            var result = new PagedResult<WheelPlayDto>
            {
                Items = pagedResult.Items,
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return new GeneralResult<PagedResult<WheelPlayDto>>(
                true,
                _localization.GetLocalizedString("AllPlaysLoaded"),
                result);
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<WheelPlayDto>>> GetPhysicalItemPlaysByDeliveryStatusAsync(
            bool? isDelivered, PaginationRequestDto pagination, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbContext.WheelPlayers
                    .Include(x => x.Award)
                    .Include(x => x.Player)
                    .Where(x => !x.IsDeleted && x.Award.Type == AwardType.PhysicalItem);

                if (isDelivered.HasValue)
                    query = query.Where(x => x.IsDelivered == isDelivered);

                var projectedQuery = query
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

                var pagedResult = await projectedQuery.ApplyPaginationAsync(pagination, cancellationToken);

                return new GeneralResult<PagedResult<WheelPlayDto>>(
                    true,
                    _localization.GetLocalizedString("PhysicalItemPlaysLoaded"),
                    pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving physical item plays with delivery filter: {IsDelivered}", isDelivered);
                return new GeneralResult<PagedResult<WheelPlayDto>>(
                    false,
                    _localization.GetLocalizedString("PhysicalItemPlaysLoadFailed"),
                    null!,
                    ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdatePhysicalItemDeliveryStatusAsync(int playId, bool isDelivered, CancellationToken cancellationToken = default)
        {
            try
            {
                var play = await _dbContext.WheelPlayers
                    .Include(x => x.Award)
                    .FirstOrDefaultAsync(x => x.Id == playId, cancellationToken);

                if (play == null)
                {
                    _logger.LogWarning("UpdatePhysicalItemDeliveryStatusAsync: Play not found. ID={PlayId}", playId);
                    return new GeneralResult(false, _localization.GetLocalizedString("PlayNotFound"), ErrorType.NotFound);
                }

                if (play.Award.Type != AwardType.PhysicalItem)
                {
                    _logger.LogWarning("UpdatePhysicalItemDeliveryStatusAsync: Invalid award type. ID={PlayId}", playId);
                    return new GeneralResult(false, _localization.GetLocalizedString("InvalidAwardType"), ErrorType.Validation);
                }

                if (play.IsDelivered == isDelivered)
                {
                    return new GeneralResult(true, _localization.GetLocalizedString("PlayStatusUnchanged"));
                }

                play.IsDelivered = isDelivered;
                play.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("UpdatePhysicalItemDeliveryStatusAsync: Updated delivery status. PlayId={PlayId}, IsDelivered={IsDelivered}", playId, isDelivered);

                return new GeneralResult(true, _localization.GetLocalizedString("PlayStatusUpdated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePhysicalItemDeliveryStatusAsync: Unexpected error. PlayId={PlayId}", playId);
                return new GeneralResult(false, _localization.GetLocalizedString("UpdateDeliveryStatusFailed"), ErrorType.InternalServerError);
            }
        }
    }
}
