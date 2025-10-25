using Lumora.DTOs.Payment;
using Lumora.Interfaces.PaymentIntf;
using Lumora.Interfaces.ProgramIntf;
using Lumora.Interfaces.UserIntf;
namespace Lumora.Services.PaymentSvc
{
    public class PaymentService(
        ILogger<PaymentService> logger,
        IRefundRepository refundRepository,
        IUserRepository userRepository,
        IPaymentRepository paymentRepository,
        PaymentMessage messages,
        IPaymentGatewayAdapter gatewayAdapter,
        IEnrollmentService enrollmentService,
        IConfiguration configuration) : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger = logger;
        private readonly PaymentMessage _messages = messages;
        private readonly IPaymentGatewayAdapter _gatewayAdapter = gatewayAdapter;
        private readonly IEnrollmentService _enrollmentService = enrollmentService;
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IRefundRepository _refundRepository = refundRepository;
        private readonly IConfiguration _configuration = configuration;

        /// <inheritdoc />
        public async Task<GeneralResult<PaymentStartResultDto>> StartPaymentAsync(PaymentCreateDto dto)
        {
            try
            {
                // تحقق من صلاحية المستخدم
                if (string.IsNullOrWhiteSpace(dto.UserId))
                {
                    _logger.LogWarning("StartPaymentAsync: Invalid userId.");
                    return new GeneralResult<PaymentStartResultDto>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var userExists = await _userRepository.ExistsAsync(dto.UserId);
                if (!userExists)
                {
                    _logger.LogWarning("StartPaymentAsync: User not found. ID: {UserId}", dto.UserId);
                    return new GeneralResult<PaymentStartResultDto>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // إنشاء كائن الدفع
                var payment = new Payment
                {
                    UserId = dto.UserId,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    Status = PaymentStatus.Pending,
                    PaymentGateway = dto.PaymentGateway,
                    Metadata = dto.Metadata,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Items = dto.Items.Select(i => new PaymentItem
                    {
                        ItemId = i.ItemId,
                        ItemType = i.ItemType,
                        Amount = i.Amount
                    }).ToList()
                };

                await _paymentRepository.AddAsync(payment);
                _logger.LogInformation(">>> Before SaveChangesAsync");
                await _paymentRepository.SaveChangesAsync();
                _logger.LogInformation(">>> After SaveChangesAsync");

                // تحضير الطلب إلى بوابة الدفع
                var request = new PaymentRequestDto
                {
                    UserEmail = dto.UserEmail,
                    UserName = dto.UserName,
                    Currency = dto.Currency,
                    Amount = dto.Amount,
                    ReferenceId = $"PAY-{payment.Id}",
                    SiteUrl = dto.SiteUrl,
                    ReturnUrl = dto.ReturnUrl,
                    ItemNames = dto.Items.Select(i => $"{i.ItemType}-{i.ItemId}").ToList(),
                    CallbackUrl = $"{dto.SiteUrl}/wejha/payment/callback",
                    ProfileId = _configuration["PayTabs:ProfileId"] ?? throw new InvalidOperationException("Missing PayTabs:ProfileId in configuration")
                };

                _logger.LogInformation(">>> Before calling gatewayAdapter.InitiateAsync");
                _logger.LogInformation("Using ProfileId = {ProfileId}", request.ProfileId);
                var gatewayResponse = await _gatewayAdapter.InitiateAsync(request);
                _logger.LogInformation(">>> After calling gatewayAdapter.InitiateAsync");
                if (!gatewayResponse.IsSuccess || gatewayResponse.Data == null)
                {
                    _logger.LogWarning("StartPaymentAsync: Failed to initiate payment. ID: {Id}", payment.Id);
                    return new GeneralResult<PaymentStartResultDto>(false, _messages.MsgGatewayInitiationFailed, null, ErrorType.BadRequest);
                }

                // حفظ المرجع
                payment.GatewayReferenceId = gatewayResponse.Data.GatewayReferenceId;
                await _paymentRepository.SaveChangesAsync();

                _logger.LogInformation("StartPaymentAsync: Payment initiated successfully. ID: {Id}, Redirect: {Url}", payment.Id, gatewayResponse.Data.RedirectUrl);

                var result = new PaymentStartResultDto
                {
                    PaymentId = payment.Id,
                    RedirectUrl = gatewayResponse.Data.RedirectUrl ?? "N/A"
                };

                return new GeneralResult<PaymentStartResultDto>(true, _messages.MsgPaymentStarted, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StartPaymentAsync: Unexpected error.");
                return new GeneralResult<PaymentStartResultDto>(false, _messages.GetUnexpectedErrorMessage("start payment"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<PaymentDto>> CreatePaymentAsync(PaymentCreateDto dto)
        {
            try
            {
                // check if user is excists.
                if(string.IsNullOrEmpty(dto.UserId) || string.IsNullOrWhiteSpace(dto.UserId))
                {
                    _logger.LogWarning("PaymentService - CreatePaymentAsync: invalid user id: {UserId}", dto.UserId);
                    return new GeneralResult<PaymentDto>(false, _messages.MsgIdInvalid, null, ErrorType.NotFound);
                }

                var userExists = await _userRepository.ExistsAsync(dto.UserId);
                if (!userExists)
                {
                    _logger.LogWarning("PaymentService - CreatePaymentAsync: User not found: {UserId}", dto.UserId);
                    return new GeneralResult<PaymentDto>(false, _messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // create payment object.
                var payment = new Payment
                {
                    UserId = dto.UserId,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    Status = PaymentStatus.Paid, // paid by default
                    PaymentGateway = dto.PaymentGateway,
                    Metadata = dto.Metadata,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Items = dto.Items.Select(i => new PaymentItem
                    {
                        ItemType = i.ItemType,
                        ItemId = i.ItemId,
                        Amount = i.Amount
                    }).ToList()
                };

                await _paymentRepository.AddAsync(payment);
                await _paymentRepository.SaveChangesAsync();

                _logger.LogInformation("PaymentService - Created new payment with ID: {PaymentId}", payment.Id);

                // response.
                var result = new PaymentDto
                {
                    Id = payment.Id,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = payment.Status,
                    PaymentGateway = payment.PaymentGateway,
                    CreatedAt = payment.CreatedAt ?? DateTimeOffset.UtcNow,
                    PaidAt = payment.PaidAt
                };

                return new GeneralResult<PaymentDto>(true, _messages.MsgPaymentCreated, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PaymentService - CreatePaymentAsync: Unexpected error");
                return new GeneralResult<PaymentDto>(false, _messages.GetUnexpectedErrorMessage("create payment"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<PaymentStatus>> VerifyPaymentStatusAsync(string gatewayReferenceId)
        {
            try
            {
                var payment = await _paymentRepository.GetByGatewayReferenceAsync(gatewayReferenceId);

                if (payment == null)
                {
                    _logger.LogWarning("VerifyPaymentStatusAsync: Payment not found for reference {Ref}", gatewayReferenceId);
                    return new GeneralResult<PaymentStatus>(
                        false, _messages.MsgPaymentNotFound, PaymentStatus.Failed, ErrorType.NotFound);
                }

                var gatewayStatus = await _gatewayAdapter.CheckStatusAsync(gatewayReferenceId);

                if (payment.Status != gatewayStatus.Data)
                {
                    payment.Status = gatewayStatus.Data;

                    if (gatewayStatus.Data == PaymentStatus.Paid)
                        payment.PaidAt = DateTime.UtcNow;

                    await _paymentRepository.SaveChangesAsync();
                    _logger.LogInformation("VerifyPaymentStatusAsync: Updated payment status for ID {Id} to {Status}", payment.Id, gatewayStatus);
                }

                return new GeneralResult<PaymentStatus>(
                    true, _messages.MsgPaymentStatusChecked, payment.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyPaymentStatusAsync: Unexpected error");
                return new GeneralResult<PaymentStatus>(
                    false, _messages.GetUnexpectedErrorMessage("verify payment"), PaymentStatus.Failed, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> LinkPaymentToItemsAsync(int paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId, includeItems: true);

                if (payment == null)
                {
                    _logger.LogWarning("LinkPaymentToItemsAsync: Payment not found. ID: {Id}", paymentId);
                    return new GeneralResult(false, _messages.MsgPaymentNotFound, null, ErrorType.NotFound);
                }

                if (payment.Status != PaymentStatus.Paid)
                {
                    _logger.LogWarning("LinkPaymentToItemsAsync: Payment is not completed. ID: {Id}", paymentId);
                    return new GeneralResult(false, _messages.MsgPaymentNotCompleted, null, ErrorType.BadRequest);
                }

                foreach (var item in payment.Items)
                {
                    switch (item.ItemType)
                    {
                        case PaymentItemType.Course:
                            await HandleCourseEnrollmentAsync(payment.UserId, item.ItemId);
                            break;

                        case PaymentItemType.Program:
                            await HandleProgramEnrollmentAsync(payment.UserId, item.ItemId);
                            break;

                        default:
                            _logger.LogWarning("LinkPaymentToItemsAsync: Unsupported item type {ItemType}", item.ItemType);
                            break;
                    }
                }

                _logger.LogInformation("LinkPaymentToItemsAsync: Payment items linked successfully for PaymentId: {Id}", paymentId);
                return new GeneralResult(true, _messages.MsgPaymentLinkedToItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LinkPaymentToItemsAsync: Unexpected error while linking items to payment ID: {Id}", paymentId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("link payment to items"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<RefundDto>> RefundAsync(RefundRequestDto dto)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(dto.PaymentId, includeItems: false);

                if (payment == null)
                {
                    _logger.LogWarning("RefundAsync: Payment not found. ID: {Id}", dto.PaymentId);
                    return new GeneralResult<RefundDto>(false, _messages.MsgPaymentNotFound, null, ErrorType.NotFound);
                }

                if (payment.Status != PaymentStatus.Paid)
                {
                    _logger.LogWarning("RefundAsync: Cannot refund unpaid payment. ID: {Id}", dto.PaymentId);
                    return new GeneralResult<RefundDto>(false, _messages.MsgPaymentNotCompleted, null, ErrorType.BadRequest);
                }

                var totalRefunded = await _refundRepository.GetTotalRefundedAmountAsync(dto.PaymentId);

                if (dto.Amount <= 0 || dto.Amount > (payment.Amount - totalRefunded))
                {
                    _logger.LogWarning("RefundAsync: Invalid refund amount for payment ID {Id}", dto.PaymentId);
                    return new GeneralResult<RefundDto>(false, _messages.MsgRefundAmountInvalid, null, ErrorType.BadRequest);
                }

                // Simulate refund with gateway – will be replaced by actual gatewayAdapter
                var simulatedSuccess = await _gatewayAdapter.RefundAsync(payment.GatewayReferenceId!, dto.Amount);
                if (simulatedSuccess.IsSuccess == false)
                {
                    _logger.LogError("RefundAsync: Gateway refund failed.");
                    return new GeneralResult<RefundDto>(false, _messages.MsgGatewayRefundFailed, null, ErrorType.BadRequest);
                }

                var refund = new Refund
                {
                    PaymentId = dto.PaymentId,
                    Amount = dto.Amount,
                    Reason = dto.Reason,
                    CreatedAt = DateTime.UtcNow,
                    Status = RefundStatus.Completed
                };

                await _refundRepository.AddAsync(refund);

                if ((totalRefunded + dto.Amount) == payment.Amount)
                {
                    payment.Status = PaymentStatus.Refunded;
                }

                await _refundRepository.SaveChangesAsync();
                await _paymentRepository.SaveChangesAsync();

                var result = new RefundDto
                {
                    Id = refund.Id,
                    PaymentId = refund.PaymentId,
                    Amount = refund.Amount,
                    Reason = refund.Reason,
                    Status = refund.Status,
                    CreatedAt = refund.CreatedAt ?? DateTimeOffset.UtcNow
                };

                _logger.LogInformation("RefundAsync: Refund completed. PaymentId: {PaymentId}, Amount: {Amount}", dto.PaymentId, dto.Amount);
                return new GeneralResult<RefundDto>(true, _messages.MsgRefundSuccess, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefundAsync: Unexpected error.");
                return new GeneralResult<RefundDto>(false, _messages.GetUnexpectedErrorMessage("refund"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<PaymentDetailsDto>> GetPaymentDetailsAsync(int paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId, includeItems: true);

                if (payment == null)
                {
                    _logger.LogWarning("GetPaymentDetailsAsync: Payment not found. ID: {Id}", paymentId);
                    return new GeneralResult<PaymentDetailsDto>(false, _messages.MsgPaymentNotFound, null, ErrorType.NotFound);
                }

                var dto = new PaymentDetailsDto
                {
                    Id = payment.Id,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = payment.Status,
                    PaymentGateway = payment.PaymentGateway,
                    CreatedAt = payment.CreatedAt ?? DateTimeOffset.UtcNow,
                    PaidAt = payment.PaidAt,
                    GatewayReferenceId = payment.GatewayReferenceId,
                    Metadata = payment.Metadata,
                    Items = payment.Items.Select(i => new PaymentItemDto
                    {
                        ItemId = i.ItemId,
                        ItemType = i.ItemType,
                        Amount = i.Amount
                    }).ToList()
                };

                _logger.LogInformation("GetPaymentDetailsAsync: Retrieved payment details successfully. ID: {Id}", paymentId);

                return new GeneralResult<PaymentDetailsDto>(true, _messages.MsgPaymentDetailsRetrieved, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPaymentDetailsAsync: Unexpected error while retrieving payment ID {Id}", paymentId);
                return new GeneralResult<PaymentDetailsDto>(false, _messages.GetUnexpectedErrorMessage("payment details retrieval"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<List<PaymentDto>>> GetUserPaymentsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("GetUserPaymentsAsync: Invalid userId.");
                    return new GeneralResult<List<PaymentDto>>(false, _messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var payments = await _paymentRepository.GetUserPaymentsAsync(userId);

                if (!payments.Any())
                {
                    _logger.LogInformation("GetUserPaymentsAsync: No payments found for user {UserId}.", userId);
                    return new GeneralResult<List<PaymentDto>>(false, _messages.MsgNoPaymentsFound, null, ErrorType.NotFound);
                }

                var result = payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status,
                    PaymentGateway = p.PaymentGateway,
                    CreatedAt = p.CreatedAt ?? DateTimeOffset.UtcNow,
                    PaidAt = p.PaidAt
                }).ToList();

                _logger.LogInformation("GetUserPaymentsAsync: Retrieved {Count} payments for user {UserId}.", result.Count, userId);
                return new GeneralResult<List<PaymentDto>>(true, _messages.MsgPaymentsRetrieved, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserPaymentsAsync: Unexpected error for user {UserId}", userId);
                return new GeneralResult<List<PaymentDto>>(
                    false, _messages.GetUnexpectedErrorMessage("payment retrieval for user"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> CancelPendingPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId, includeItems: false);

                if (payment == null)
                {
                    _logger.LogWarning("CancelPendingPaymentAsync: Payment not found. ID: {Id}", paymentId);
                    return new GeneralResult(false, _messages.MsgPaymentNotFound, null, ErrorType.NotFound);
                }

                if (payment.Status != PaymentStatus.Pending)
                {
                    _logger.LogWarning("CancelPendingPaymentAsync: Cannot cancel payment with status {Status}. ID: {Id}", payment.Status, paymentId);
                    return new GeneralResult(false, _messages.MsgOnlyPendingCanBeCancelled, null, ErrorType.BadRequest);
                }

                payment.Status = PaymentStatus.Cancelled;
                await _paymentRepository.SaveChangesAsync();

                _logger.LogInformation("CancelPendingPaymentAsync: Payment cancelled successfully. ID: {Id}", paymentId);
                return new GeneralResult(true, _messages.MsgPaymentCancelled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancelPendingPaymentAsync: Unexpected error. ID: {Id}", paymentId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("payment cancellation"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<bool>> HasUserPaidForAsync(string userId, PaymentItemType itemType, int itemId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || itemId <= 0)
                {
                    _logger.LogWarning("HasUserPaidForAsync: Invalid input. UserId: {UserId}, ItemId: {ItemId}", userId, itemId);
                    return new GeneralResult<bool>(false, _messages.MsgIdInvalid, false, ErrorType.BadRequest);
                }

                var hasPaid = await _paymentRepository.UserHasPaidForAsync(userId, itemType, itemId);

                if (!hasPaid)
                {
                    _logger.LogInformation("HasUserPaidForAsync: No payment found for UserId {UserId}, ItemType {Type}, ItemId {ItemId}",
                        userId, itemType, itemId);
                    return new GeneralResult<bool>(true, _messages.MsgPaymentNotFoundForItem, false);
                }

                _logger.LogInformation("HasUserPaidForAsync: User {UserId} has paid for ItemType {Type} and ItemId {ItemId}",
                    userId, itemType, itemId);

                return new GeneralResult<bool>(true, _messages.MsgPaymentFoundForItem, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HasUserPaidForAsync: Unexpected error for user {UserId} and item {ItemId}", userId, itemId);
                return new GeneralResult<bool>(false, _messages.GetUnexpectedErrorMessage("payment check"), false, ErrorType.InternalServerError);
            }
        }

        private async Task HandleCourseEnrollmentAsync(string userId, int courseId)
        {
            var result = await _enrollmentService.EnrollInProgramAsync(courseId, userId, CancellationToken.None);
            if (result.IsSuccess == false)
            {
                _logger.LogWarning("Program enrollment failed after payment. ProgramId: {ProgramId}, UserId: {UserId}, Reason: {Reason}", courseId, userId, result.Message);
            }
        }
        private async Task HandleProgramEnrollmentAsync(string userId, int programId)
        {
            var result = await _enrollmentService.EnrollInProgramAsync(programId, userId, CancellationToken.None);
            if (result.IsSuccess == false)
            {
                _logger.LogWarning("Program enrollment failed after payment. ProgramId: {ProgramId}, UserId: {UserId}, Reason: {Reason}", programId, userId, result.Message);
            }
        }
    }
}
