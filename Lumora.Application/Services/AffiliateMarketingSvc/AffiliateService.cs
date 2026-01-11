namespace Lumora.Application.Services.AffiliateMarketingSvc
{
    public class AffiliateService : IAffiliateService
    {
        private readonly IAffiliateRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AffiliateService> _logger;
        private readonly AffiliateMessage _messages;

        public AffiliateService(IAffiliateRepository repository, IUnitOfWork unitOfWork, ILogger<AffiliateService> logger, AffiliateMessage messages)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _messages = messages;
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> CreatePromoCodeAsync(PromoCodeCreateDto dto, string userId, CancellationToken cancellationToken, string createdByIp, string createdByAgent)
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

                /* Fetching user through repository abstraction */
                var user = await _repository.GetUserByIdAsync(userId, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("{Method} - User not found. UserId: {UserId}", method, userId);
                    return new GeneralResult(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                /* Validating training program existence via repository */
                var trainingProgram = await _repository.GetProgramByIdAsync(dto.TrainingProgramId, cancellationToken);

                if (trainingProgram == null)
                {
                    _logger.LogWarning("{Method} - Training program not found. ProgramId: {ProgramId}", method, dto.TrainingProgramId);
                    return new GeneralResult(false, _messages.MsgTrainingProgramNotFound, null, ErrorType.NotFound);
                }

                var codeToUse = string.IsNullOrWhiteSpace(dto.Code)
                    ? $"PC-{Guid.NewGuid():N}".Substring(0, 10).ToUpperInvariant()
                    : dto.Code.Trim().ToUpperInvariant();

                /* Checking for duplicates using repository logic */
                var isDuplicate = await _repository.IsPromoCodeDuplicateAsync(codeToUse, cancellationToken);

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
                    ByIp = createdByIp,
                    ByAgent = createdByAgent
                };

                /* Persisting new entity through repository and confirming changes */
                await _repository.AddPromoCodeAsync(promoCode, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        public async Task<GeneralResult> RegisterPromoCodeUsageAsync(int paymentId, CancellationToken cancellationToken, string createdByIp, string createdByAgent)
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

                // Fetch payment including promo code details via repository
                var payment = await _repository.GetPaymentWithPromoCodeAsync(paymentId, cancellationToken);

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

                // Check for existing usage via repository
                var alreadyUsed = await _repository.IsUsageRegisteredAsync(paymentId, cancellationToken);

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
                    ByIp = createdByIp,
                    ByAgent = createdByAgent
                };

                // Persist data using repository methods
                await _repository.AddPromoCodeUsageAsync(usage, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        public async Task<GeneralResult> DeactivateAllPromoCodesAsync(string performedByUserId, CancellationToken cancellationToken, string createdByIp, string createdByAgent)
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

                /* Retrieve active promo codes via the repository abstraction */
                var promoCodes = await _repository.GetActivePromoCodesAsync(cancellationToken);

                if (promoCodes == null || !promoCodes.Any())
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
                    promoCode.ByIp = createdByIp;
                    promoCode.ByAgent = createdByAgent;
                }

                /* Persist changes through the repository */
                await _repository.UpdatePromoCodesAsync(promoCodes, cancellationToken);

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
                /* Use repository to fetch pre-projected report data to keep application layer clean from EF logic */
                var report = await _repository.GetPromoCodeReportAsync(cancellationToken);

                if (report == null || !report.Any())
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

                /* Verify user existence via repository */
                var userExists = await _repository.UserExistsAsync(userId, cancellationToken);

                if (!userExists)
                {
                    _logger.LogWarning("{Method} - User not found. UserId: {UserId}", method, userId);
                    return new GeneralResult<List<PromoCodeReportDto>>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                /* Retrieve the data transfer objects through the repository abstraction */
                var promoCodes = await _repository.GetPromoCodesByUserAsync(userId, cancellationToken);

                if (promoCodes == null || !promoCodes.Any())
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
        public async Task<GeneralResult> ReactivatePromoCodeAsync(int promoCodeId, string performedByUserId, CancellationToken cancellationToken, string createdByIp, string createdByAgent)
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

                /* Retrieve the entity via the repository abstraction */
                var promoCode = await _repository.GetPromoCodeByIdAsync(promoCodeId, cancellationToken);

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
                promoCode.ByIp = createdByIp;
                promoCode.ByAgent = createdByAgent;

                /* Prepare the entity for updating and persist changes through the repository */
                _repository.UpdatePromoCode(promoCode);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
