using Lumora.DTOs.Payment;
using Lumora.Interfaces.PaymentIntf;

namespace Lumora.Web.Controllers.PaymentAPI
{
    [Route("wejha/api/[controller]")]
    [ApiController]
    public class PaymentController(ILogger<PaymentController> logger, IPaymentService paymentService) : AuthenticatedController
    {
        private readonly IPaymentService _paymentService = paymentService;
        private readonly ILogger<PaymentController> _logger = logger;

        //[HttpPost("start")]
        //public async Task<IActionResult> StartPayment([FromBody] PaymentCreateDto dto)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        return Unauthorized(new GeneralResult { IsSuccess = false, Message = "User not logged in." });
        //    }

        //    dto.UserId = userId;
        //    _logger.LogInformation(">>> From PaymentController - StartPayment endpoint : Before calling _paymentService.StartPaymentAsync");
        //    var result = await _paymentService.StartPaymentAsync(dto);
        //    _logger.LogInformation(">>> From PaymentController - StartPayment endpoint : After calling _paymentService.StartPaymentAsync");
        //    return Ok(result);
        //}

        [HttpPost("create")]
        [ProducesResponseType(typeof(GeneralResult<PaymentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateDto dto)
        {
            var userCheck = CheckUserOrUnauthorized();
            if (userCheck != null) return userCheck;

            dto.UserId = CurrentUserId!;
            var result = await _paymentService.CreatePaymentAsync(dto);
            if (result.IsSuccess == false)
            {
                return result.ErrorType switch
                {
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }

            //if (result.Data!.PaymentPurpose == PaymentPurpose.Subscription)
            //{
            //    await CreateSubscriptionAfterPaymentAsync(paymentDto);
            //}

            //if (result.Data!.PaymentPurpose == PaymentPurpose.Buy)
            //{
            //    await CreatePurchaseAfterPaymentAsync(result);
            //}

            return Ok(result);
        }

        //[HttpGet("verify/{gatewayReferenceId}")]
        //[ProducesResponseType(typeof(GeneralResult<PaymentStatus>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> VerifyStatus(string gatewayReferenceId)
        //{
        //    var result = await _paymentService.VerifyPaymentStatusAsync(gatewayReferenceId);
        //    if (result.IsSuccess == false)
        //    {
        //        return result.ErrorType switch
        //        {
        //            ErrorType.BadRequest => BadRequest(result),
        //            ErrorType.NotFound => NotFound(result),
        //            ErrorType.InternalServerError => StatusCode(500, result),
        //            _ => BadRequest(result)
        //        };
        //    }

        //    return Ok(result);
        //}

        //[HttpPost("link/{paymentId}")]
        //[ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        //public async Task<IActionResult> LinkPayment(int paymentId)
        //{
        //    var result = await _paymentService.LinkPaymentToItemsAsync(paymentId);
        //    if (result.IsSuccess == false)
        //    {
        //        return result.ErrorType switch
        //        {
        //            ErrorType.BadRequest => BadRequest(result),
        //            ErrorType.NotFound => NotFound(result),
        //            ErrorType.InternalServerError => StatusCode(500, result),
        //            _ => BadRequest(result)
        //        };
        //    }

        //    return Ok(result);
        //}

        //[HttpPost("refund")]
        //[ProducesResponseType(typeof(GeneralResult<RefundDto>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> Refund([FromBody] RefundRequestDto dto)
        //{
        //    var result = await _paymentService.RefundAsync(dto);
        //    if (result.IsSuccess == false)
        //    {
        //        return result.ErrorType switch
        //        {
        //            ErrorType.BadRequest => BadRequest(result),
        //            ErrorType.NotFound => NotFound(result),
        //            ErrorType.InternalServerError => StatusCode(500, result),
        //            _ => BadRequest(result)
        //        };
        //    }

        //    return Ok(result);
        //}

        //[HttpGet("details/{paymentId}")]
        //[ProducesResponseType(typeof(GeneralResult<PaymentDetailsDto>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> GetDetails(int paymentId)
        //{
        //    var result = await _paymentService.GetPaymentDetailsAsync(paymentId);
        //    if (result.IsSuccess == false)
        //    {
        //        return result.ErrorType switch
        //        {
        //            ErrorType.BadRequest => BadRequest(result),
        //            ErrorType.NotFound => NotFound(result),
        //            ErrorType.InternalServerError => StatusCode(500, result),
        //            _ => BadRequest(result)
        //        };
        //    }

        //    return Ok(result);
        //}

        //[HttpGet("user/{userId}")]
        //[ProducesResponseType(typeof(GeneralResult<List<PaymentDto>>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> GetUserPayments(string userId)
        //{
        //    var result = await _paymentService.GetUserPaymentsAsync(userId);
        //    if (result.IsSuccess == false)
        //    {
        //        return result.ErrorType switch
        //        {
        //            ErrorType.BadRequest => BadRequest(result),
        //            ErrorType.NotFound => NotFound(result),
        //            ErrorType.InternalServerError => StatusCode(500, result),
        //            _ => BadRequest(result)
        //        };
        //    }

        //    return Ok(result);
        //}

        //[HttpPost("cancel/{paymentId}")]
        //[ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        //public async Task<IActionResult> Cancel(int paymentId)
        //{
        //    var result = await _paymentService.CancelPendingPaymentAsync(paymentId);
        //    if (result.IsSuccess == false)
        //    {
        //        return result.ErrorType switch
        //        {
        //            ErrorType.BadRequest => BadRequest(result),
        //            ErrorType.NotFound => NotFound(result),
        //            ErrorType.InternalServerError => StatusCode(500, result),
        //            _ => BadRequest(result)
        //        };
        //    }

        //    return Ok(result);
        //}

        //[HttpGet("has-paid")]
        //[ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status200OK)]
        //public async Task<IActionResult> HasPaid([FromQuery] string userId, [FromQuery] PaymentItemType itemType, [FromQuery] int itemId)
        //{
        //    var result = await _paymentService.HasUserPaidForAsync(userId, itemType, itemId);
        //    if (result.IsSuccess == false)
        //    {
        //        return result.ErrorType switch
        //        {
        //            ErrorType.BadRequest => BadRequest(result),
        //            ErrorType.NotFound => NotFound(result),
        //            ErrorType.InternalServerError => StatusCode(500, result),
        //            _ => BadRequest(result)
        //        };
        //    }

        //    return Ok(result);
        //}

        //[HttpPost("callback")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<IActionResult> PaymentCallback([FromBody] PayTabsV3QueryResponse payload)
        //{
        //    if (payload == null || string.IsNullOrWhiteSpace(payload.TransactionReference))
        //    {
        //        _logger.LogWarning("PaymentCallback: Missing or invalid transaction reference.");
        //        return BadRequest("Missing transaction reference");
        //    }

        //    _logger.LogInformation("PaymentCallback: Received callback for transaction {TranRef}", payload.TransactionReference);

        //    var result = await _paymentService.VerifyPaymentStatusAsync(payload.TransactionReference);

        //    if (!result.IsSuccess)
        //    {
        //        _logger.LogWarning("PaymentCallback: Verification failed for {TranRef}. Reason: {Reason}", payload.TransactionReference, result.Message);
        //    }
        //    else
        //    {
        //        _logger.LogInformation("PaymentCallback: Verification succeeded for {TranRef}. Status: {Status}", payload.TransactionReference, result.Data);
        //    }

        //    return Ok(); // Always return 200 OK to avoid PayTabs retries
        //}

        /////// <summary>
        /////// Create subscription after payment.
        /////// </summary>
        ////private async Task CreateSubscriptionAfterPaymentAsync(PaymentDetailsDto payment)
        ////{
        ////    var type = (SubscriptionType)(int)payment.TargetType!;

        ////    var dto = new SubscriptionCreateDto
        ////    {
        ////        UserId = payment.UserId,
        ////        Type = type,
        ////        ReferenceId = payment.TargetId,
        ////        Price = payment.NetAmount ?? 0,
        ////        PaymentId = payment.Id,
        ////        IsAutoRenewal = false
        ////    };

        ////    await _subscriptionService.CreateSubscriptionAsync(dto);
        ////}

        ///// <summary>
        ///// Creates a purchase record after successful payment (PaymentPurpose.Buy).
        ///// </summary>
        //private async Task CreatePurchaseAfterPaymentAsync(PaymentDetailsDto payment)
        //{
        //    var type = (PurchaseItemType)(int)payment.TargetType;

        //    var dto = new PurchaseCreateDto
        //    {
        //        UserId = payment.UserId,
        //        ItemType = type,
        //        ReferenceId = payment.TargetId ?? -1,
        //        Price = payment.NetAmount ?? 0,
        //        PaymentId = payment.Id,
        //        MetadataJson = null
        //    };

        //    await _purchaseService.CreatePurchaseAsync(dto);
        //}
    }
}
