namespace Lumora.Application.Services.ClubIntf
{
    public class WheelPlayerService(
            IWheelRepository wheelRepository,
            ILocalizationManager localization,
            ILogger<WheelPlayerService> logger,
            IUnitOfWork unitOfWork) : IWheelPlayerService
    {
        private readonly IWheelRepository _wheelRepository = wheelRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILocalizationManager _localization = localization;
        private readonly ILogger<WheelPlayerService> _logger = logger;

        /// <inheritdoc/>
        public async Task<GeneralResult> CanPlayTodayAsync(string playerId)
        {
            try
            {
                var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
                var tomorrowStart = todayStart.AddDays(1);

                /* Fetch current plays count for the player today via repository */
                var todayPlays = await _wheelRepository.GetPlayerSpinCountAsync(playerId, todayStart, tomorrowStart);

                /* Fetch successful paid retries count for the player today via repository */
                var paidRetries = await _wheelRepository.GetPaidRetriesCountAsync(playerId, todayStart, tomorrowStart);

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
        public async Task<GeneralResult> SpinAsync(string playerId, int awardId, string deviceInfo, string ipAddress)
        {
            try
            {
                var today = DateTimeOffset.UtcNow.Date.ToUniversalTime();

                // Attempt to retrieve existing state or initialize a new one
                var state = await _wheelRepository.GetPlayerStateAsync(playerId, today);

                if (state == null)
                {
                    state = new WheelPlayerState
                    {
                        PlayerId = playerId,
                        Date = today,
                        HasUsedFreeSpin = false,
                        AllowPaidSpin = false
                    };

                    await _wheelRepository.AddPlayerStateAsync(state);
                    // Persistence is required here to ensure state exists for subsequent logic
                    await _unitOfWork.SaveChangesAsync();
                }

                // Validate spin eligibility
                if (state.HasUsedFreeSpin && !state.AllowPaidSpin)
                {
                    return new GeneralResult
                    {
                        IsSuccess = false,
                        Message = _localization.GetLocalizedString("AlreadyPlayedToday"),
                        Data = null
                    };
                }

                // Extract connection details for security auditing
                var device = deviceInfo;
                var ip = ipAddress;

                var duplicateAccounts = await _wheelRepository.GetPlayerIdsByConnectionDetailsAsync(ip, device);
                if (duplicateAccounts?.Count > 1)
                {
                    _logger.LogWarning("Suspicious spin: multiple accounts using same device/IP. PlayerId: {PlayerId}, IP: {Ip}, Device: {Device}", playerId, ip, device);
                }

                // Verify award availability
                var existsAward = await _wheelRepository.GetAwardByIdAsync(awardId);
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

                await _wheelRepository.AddWheelPlayerAsync(entry);

                // Update state flags based on spin type
                if (isFree)
                    state.HasUsedFreeSpin = true;
                else
                    state.AllowPaidSpin = false;

                await _wheelRepository.UpdatePlayerStateAsync(state);
                await _unitOfWork.SaveChangesAsync();

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
                // Fetch history via repository
                var history = await _wheelRepository.GetPlayerHistorySimpleAsync(playerId);

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

                /* Fetch data using repository projection to maintain layer isolation */
                var spins = await _wheelRepository.GetPlayerSpinsInDateRangeAsync(
                    playerId,
                    today,
                    tomorrow,
                    x => new
                    {
                        x.AwardId,
                        AwardName = x.Award.Name,
                        x.PlayedAt,
                        x.IsFree,
                        x.DeviceInfo,
                        x.IpAddress
                    });

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
            var today = DateTimeOffset.UtcNow.Date;

            // Fetch the current player state from the repository
            var state = await _wheelRepository.GetPlayerStateAsync(playerId, today);

            if (state == null)
            {
                var newState = new WheelPlayerState
                {
                    PlayerId = playerId,
                    Date = today,
                    HasUsedFreeSpin = false,
                    AllowPaidSpin = false
                };

                // Persist the new state via repository
                await _wheelRepository.AddPlayerStateAsync(newState);

                // Commit changes through Unit of Work
                await _unitOfWork.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task ActivatePaidSpinAsync(string playerId)
        {
            var today = DateTimeOffset.UtcNow.Date;

            /* Fetch the player state through the repository */
            var state = await _wheelRepository.GetPlayerStateAsync(playerId, today);

            if (state != null)
            {
                state.AllowPaidSpin = true;

                /* Mark the entity as modified in the repository */
                await _wheelRepository.UpdatePlayerStateAsync(state);

                /* Commit changes via Unit of Work */
                await _unitOfWork.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> MarkPlayDeliveredAsync(int playId, bool isDelivered)
        {
            /* Fetch play record using repository including necessary navigation properties */
            var play = await _wheelRepository.GetWheelPlayerByIdAsync(playId);

            if (play == null)
            {
                _logger.LogWarning("MarkPlayDeliveredAsync: Play not found. ID={PlayId}", playId);
                return new GeneralResult(false, _localization.GetLocalizedString("PlayNotFound"));
            }

            /* Validate if the award associated with this play is a physical item */
            if (play.Award.Type != AwardType.PhysicalItem)
            {
                _logger.LogWarning("MarkPlayDeliveredAsync: Attempted to mark non-physical award as delivered. ID={PlayId}", playId);
                return new GeneralResult(false, _localization.GetLocalizedString("InvalidAwardType"));
            }

            /* Skip update if the status is already set to the target value */
            if (play.IsDelivered == isDelivered)
            {
                return new GeneralResult(true, _localization.GetLocalizedString("PlayStatusUnchanged"));
            }

            /* Apply changes to the entity properties */
            play.IsDelivered = isDelivered;
            play.UpdatedAt = DateTimeOffset.UtcNow;

            /* Persist changes through the repository layer */
            await _wheelRepository.UpdateWheelPlayerAsync(play);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("MarkPlayDeliveredAsync: Updated delivery status for PlayId={PlayId} to {Status}", playId, isDelivered);

            return new GeneralResult(true, _localization.GetLocalizedString("PlayStatusUpdated"));
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<WheelPlayDto>>> GetPlaysByDeliveryStatusAsync(
            bool delivered, PaginationRequestDto pagination, CancellationToken cancellationToken = default)
        {
            try
            {
                var pagedResult = await _wheelRepository.GetPlaysByDeliveryStatusPagedAsync(
                    delivered, pagination.PageNumber, pagination.PageSize, cancellationToken);

                return new GeneralResult<PagedResult<WheelPlayDto>>(
                    true,
                    _localization.GetLocalizedString("PlaysByDeliveryLoaded"),
                    pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plays by delivery status: {Status}", delivered);
                return new GeneralResult<PagedResult<WheelPlayDto>>(false, _localization.GetLocalizedString("GeneralError"), null!);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<WheelPlayDto>>> GetAllUserPlaysAsync(
            PaginationRequestDto pagination, CancellationToken cancellationToken = default)
        {
            try
            {
                var pagedResult = await _wheelRepository.GetAllUserPlaysPagedAsync(
                    pagination.PageNumber, pagination.PageSize, cancellationToken);

                return new GeneralResult<PagedResult<WheelPlayDto>>(
                    true,
                    _localization.GetLocalizedString("AllPlaysLoaded"),
                    pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all user plays");
                return new GeneralResult<PagedResult<WheelPlayDto>>(false, _localization.GetLocalizedString("GeneralError"), null!);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<PagedResult<WheelPlayDto>>> GetPhysicalItemPlaysByDeliveryStatusAsync(
            bool? isDelivered, PaginationRequestDto pagination, CancellationToken cancellationToken = default)
        {
            try
            {
                var pagedResult = await _wheelRepository.GetPagedPhysicalItemPlaysAsync(
                    isDelivered, pagination.PageNumber, pagination.PageSize, cancellationToken);

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
                // Use repository to fetch data with necessary inclusions
                var play = await _wheelRepository.GetWheelPlayerByIdAsync(playId);

                if (play == null)
                {
                    _logger.LogWarning("UpdatePhysicalItemDeliveryStatusAsync: Play not found. ID={PlayId}", playId);
                    return new GeneralResult(false, _localization.GetLocalizedString("PlayNotFound"), ErrorType.NotFound);
                }

                // Validate domain constraints
                if (play.Award.Type != AwardType.PhysicalItem)
                {
                    _logger.LogWarning("UpdatePhysicalItemDeliveryStatusAsync: Invalid award type. ID={PlayId}", playId);
                    return new GeneralResult(false, _localization.GetLocalizedString("InvalidAwardType"), ErrorType.Validation);
                }

                // Check if state transition is necessary
                if (play.IsDelivered == isDelivered)
                {
                    return new GeneralResult(true, _localization.GetLocalizedString("PlayStatusUnchanged"));
                }

                // Apply changes to the domain entity
                play.IsDelivered = isDelivered;
                play.UpdatedAt = DateTimeOffset.UtcNow;

                _wheelRepository.UpdateWheelPlayer(play);

                // Persist changes through repository abstraction
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
