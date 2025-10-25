using Lumora.DTOs.AffiliateMarketing;
using Lumora.Interfaces.AffiliateMarketingIntf;

namespace Lumora.Services.AffiliateMarketingSvc
{
    public class AffiliateService(
        PgDbContext dbContext, ILogger<AffiliateService> logger, AffiliateMessage messages,
        IHttpContextHelper httpContextHelper, IOptions<AffiliateSettings> settings) : IAffiliateService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<AffiliateService> _logger = logger;
        private readonly AffiliateMessage _messages = messages;
        private readonly AffiliateSettings _affiliateSettings = settings.Value;

        /// <inheritdoc/>
        public async Task<GeneralResult> CreatePromoCodeAsync(PromoCodeCreateDto dto, string userId, CancellationToken cancellationToken)
        {
            var method = nameof(CreatePromoCodeAsync);
            var now = DateTimeOffset.UtcNow;

            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("{Method} - DTO is null.", method);
                    return new GeneralResult(false, _messages.MsgDtoNull, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("{Method} - User ID is null or empty.", method);
                    return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("{Method} - User not found. UserId: {UserId}", method, userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var trainingProgram = await _dbContext.TrainingPrograms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == dto.TrainingProgramId && !p.IsDeleted, cancellationToken);

                if (trainingProgram == null)
                {
                    _logger.LogWarning("{Method} - Training program not found. ProgramId: {ProgramId}", method, dto.TrainingProgramId);
                    return new GeneralResult(false, _messages.MsgTrainingProgramNotFound, null, ErrorType.NotFound);
                }

                var codeToUse = string.IsNullOrWhiteSpace(dto.Code)
                    ? $"PC-{Guid.NewGuid():N}".Substring(0, 10).ToUpperInvariant()
                    : dto.Code.Trim().ToUpperInvariant();

                var isDuplicate = await _dbContext.PromoCodes
                    .AsNoTracking()
                    .AnyAsync(pc => pc.Code == codeToUse && !pc.IsDeleted, cancellationToken);

                if (isDuplicate)
                {
                    _logger.LogWarning("{Method} - Promo code already exists. Code: {Code}", method, codeToUse);
                    return new GeneralResult(false, _messages.MsgPromoCodeExists, null, ErrorType.BadRequest);
                }

                var promoCode = new PromoCode
                {
                    Code = codeToUse,
                    IsManual = !string.IsNullOrWhiteSpace(dto.Code),
                    UserId = userId,
                    TrainingProgramId = dto.TrainingProgramId,
                    DiscountPercentage = dto.DiscountPercentage,
                    CommissionPercentage = dto.CommissionPercentage,
                    CreatedAt = now,
                    IsActive = true,
                    ById = userId,
                    ByIp = httpContextHelper.IpAddress,
                    ByAgent = httpContextHelper.UserAgent
                };

                await _dbContext.PromoCodes.AddAsync(promoCode, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("{Method} - Promo code created successfully. Code: {Code}, UserId: {UserId}", method, codeToUse, userId);
                return new GeneralResult(true, _messages.MsgPromoCodeCreatedSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} - Unexpected error while creating promo code.", method);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Create Promo Code"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> RegisterPromoCodeUsageAsync(int paymentId, CancellationToken cancellationToken)
        {
            var method = nameof(RegisterPromoCodeUsageAsync);
            var now = DateTimeOffset.UtcNow;

            try
            {
                if (paymentId <= 0)
                {
                    _logger.LogWarning("{Method} - Payment ID is invalid.", method);
                    return new GeneralResult(false, _messages.MsgPaymentIdRequired, null, ErrorType.BadRequest);
                }

                var payment = await _dbContext.Payments
                    .Include(p => p.PromoCode)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted, cancellationToken);

                if (payment == null)
                {
                    _logger.LogWarning("{Method} - Payment not found. PaymentId: {PaymentId}", method, paymentId);
                    return new GeneralResult(false, _messages.MsgPaymentNotFound, null, ErrorType.NotFound);
                }

                if (payment.PromoCodeId == null)
                {
                    _logger.LogWarning("{Method} - No promo code attached to the payment. PaymentId: {PaymentId}", method, paymentId);
                    return new GeneralResult(false, _messages.MsgPromoCodeMissingFromPayment, null, ErrorType.BadRequest);
                }

                if (payment.PromoCode == null || !payment.PromoCode.IsActive || payment.PromoCode.IsDeleted)
                {
                    _logger.LogWarning("{Method} - Promo code is invalid or inactive. CodeId: {CodeId}", method, payment.PromoCodeId);
                    return new GeneralResult(false, _messages.MsgPromoCodeInactive, null, ErrorType.BadRequest);
                }

                var alreadyUsed = await _dbContext.PromoCodeUsages
                    .AsNoTracking()
                    .AnyAsync(u => u.PaymentId == paymentId, cancellationToken);

                if (alreadyUsed)
                {
                    _logger.LogInformation("{Method} - Promo code usage already registered for payment. PaymentId: {PaymentId}", method, paymentId);
                    return new GeneralResult(false, _messages.MsgPromoCodeUsageAlreadyExists, null, ErrorType.BadRequest);
                }

                var usage = new PromoCodeUsage
                {
                    PromoCodeId = payment.PromoCodeId.Value,
                    PaymentId = paymentId,
                    UsedAt = now,
                    CreatedAt = now,
                    ById = payment.UserId,
                    ByIp = httpContextHelper.IpAddress,
                    ByAgent = httpContextHelper.UserAgent
                };

                await _dbContext.PromoCodeUsages.AddAsync(usage, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("{Method} - Promo code usage registered successfully. PaymentId: {PaymentId}, CodeId: {CodeId}", method, paymentId, payment.PromoCodeId);
                return new GeneralResult(true, _messages.MsgPromoCodeUsageRegisteredSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} - Unexpected error while registering promo code usage.", method);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Register Promo Code Usage"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeactivateAllPromoCodesAsync(string performedByUserId, CancellationToken cancellationToken)
        {
            var method = nameof(DeactivateAllPromoCodesAsync);
            var now = DateTimeOffset.UtcNow;

            try
            {
                if (string.IsNullOrWhiteSpace(performedByUserId))
                {
                    _logger.LogWarning("{Method} - User ID is null or empty.", method);
                    return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var promoCodes = await _dbContext.PromoCodes
                    .Where(pc => pc.IsActive && !pc.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!promoCodes.Any())
                {
                    _logger.LogInformation("{Method} - No active promo codes found to deactivate.", method);
                    return new GeneralResult(false, _messages.MsgNoActivePromoCodes, null, ErrorType.NotFound);
                }

                foreach (var promoCode in promoCodes)
                {
                    promoCode.IsActive = false;
                    promoCode.DeactivatedAt = now;
                    promoCode.UpdatedAt = now;
                    promoCode.ById = performedByUserId;
                    promoCode.ByIp = httpContextHelper.IpAddress;
                    promoCode.ByAgent = httpContextHelper.UserAgent;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("{Method} - {Count} promo codes deactivated by user {UserId}", method, promoCodes.Count, performedByUserId);
                return new GeneralResult(true, _messages.MsgAllPromoCodesDeactivated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} - Unexpected error while deactivating promo codes.", method);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Deactivate Promo Codes"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<PromoCodeReportDto>>> GetPromoCodeReportAsync(CancellationToken cancellationToken)
        {
            var method = nameof(GetPromoCodeReportAsync);

            try
            {
                var report = await _dbContext.PromoCodes
                    .Where(pc => !pc.IsDeleted)
                    .Select(pc => new PromoCodeReportDto
                    {
                        Code = pc.Code,
                        UserFullName = pc.User.FullName ?? "",
                        ProgramTitle = pc.TrainingProgram.Name ?? "",
                        IsActive = pc.IsActive,
                        UsageCount = _dbContext.PromoCodeUsages.Count(u => u.PromoCodeId == pc.Id)
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                if (!report.Any())
                {
                    _logger.LogInformation("{Method} - No promo codes found.", method);
                    return new GeneralResult<List<PromoCodeReportDto>>(false, _messages.MsgNoPromoCodesFound, null, ErrorType.NotFound);
                }

                _logger.LogInformation("{Method} - Retrieved {Count} promo code records.", method, report.Count);
                return new GeneralResult<List<PromoCodeReportDto>>(true, _messages.MsgPromoCodeReportSuccess, report, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} - Unexpected error while retrieving promo code report.", method);
                return new GeneralResult<List<PromoCodeReportDto>>(false, _messages.GetUnexpectedErrorMessage("Promo Code Report"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<PromoCodeReportDto>>> GetPromoCodesByUserAsync(string userId, CancellationToken cancellationToken)
        {
            var method = nameof(GetPromoCodesByUserAsync);

            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("{Method} - User ID is null or empty.", method);
                    return new GeneralResult<List<PromoCodeReportDto>>(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var userExists = await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                if (!userExists)
                {
                    _logger.LogWarning("{Method} - User not found. UserId: {UserId}", method, userId);
                    return new GeneralResult<List<PromoCodeReportDto>>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var promoCodes = await _dbContext.PromoCodes
                    .Where(pc => pc.UserId == userId && !pc.IsDeleted)
                    .Select(pc => new PromoCodeReportDto
                    {
                        Code = pc.Code,
                        UserFullName = pc.User.FullName ?? "",
                        ProgramTitle = pc.TrainingProgram.Name ?? "",
                        IsActive = pc.IsActive,
                        UsageCount = _dbContext.PromoCodeUsages.Count(u => u.PromoCodeId == pc.Id)
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                if (!promoCodes.Any())
                {
                    _logger.LogInformation("{Method} - No promo codes found for user. UserId: {UserId}", method, userId);
                    return new GeneralResult<List<PromoCodeReportDto>>(false, _messages.MsgNoPromoCodesFound, null, ErrorType.NotFound);
                }

                _logger.LogInformation("{Method} - Retrieved {Count} promo codes for user {UserId}", method, promoCodes.Count, userId);
                return new GeneralResult<List<PromoCodeReportDto>>(true, _messages.MsgPromoCodeReportSuccess, promoCodes, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} - Unexpected error while retrieving user promo codes.", method);
                return new GeneralResult<List<PromoCodeReportDto>>(false, _messages.GetUnexpectedErrorMessage("Get User Promo Codes"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ReactivatePromoCodeAsync(int promoCodeId, string performedByUserId, CancellationToken cancellationToken)
        {
            var method = nameof(ReactivatePromoCodeAsync);
            var now = DateTimeOffset.UtcNow;

            try
            {
                if (promoCodeId <= 0)
                {
                    _logger.LogWarning("{Method} - Invalid promo code ID.", method);
                    return new GeneralResult(false, _messages.MsgPromoCodeIdRequired, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(performedByUserId))
                {
                    _logger.LogWarning("{Method} - User ID is null or empty.", method);
                    return new GeneralResult(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var promoCode = await _dbContext.PromoCodes
                    .FirstOrDefaultAsync(pc => pc.Id == promoCodeId && !pc.IsDeleted, cancellationToken);

                if (promoCode == null)
                {
                    _logger.LogWarning("{Method} - Promo code not found. Id: {PromoCodeId}", method, promoCodeId);
                    return new GeneralResult(false, _messages.MsgPromoCodeNotFound, null, ErrorType.NotFound);
                }

                if (promoCode.IsActive)
                {
                    _logger.LogInformation("{Method} - Promo code is already active. Id: {PromoCodeId}", method, promoCodeId);
                    return new GeneralResult(false, _messages.MsgPromoCodeAlreadyActive, null, ErrorType.BadRequest);
                }

                promoCode.IsActive = true;
                promoCode.DeactivatedAt = null;
                promoCode.UpdatedAt = now;
                promoCode.ById = performedByUserId;
                promoCode.ByIp = httpContextHelper.IpAddress;
                promoCode.ByAgent = httpContextHelper.UserAgent;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("{Method} - Promo code reactivated successfully. Id: {PromoCodeId}", method, promoCodeId);
                return new GeneralResult(true, _messages.MsgPromoCodeReactivated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} - Unexpected error while reactivating promo code.", method);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Reactivate Promo Code"), null, ErrorType.InternalServerError);
            }
        }
    }
}
