namespace Lumora.DTOs.Payment;

/*======================== 1. StartPaymentAsync(PaymentCreateDto dto). ===========================*/
/*======================== 2. CreatePaymentAsync(PaymentCreateDto dto). ===========================*/
public class PaymentCreateDto
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "SAR";

    public PaymentGatewayType PaymentGateway { get; set; }

    public string? Metadata { get; set; }

    public string SiteUrl { get; set; } = null!;

    public string ReturnUrl { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public List<PaymentItemCreateDto> Items { get; set; } = new();
}

public class PaymentStartResultDto
{
    public int PaymentId { get; set; }
    public string RedirectUrl { get; set; } = string.Empty;
    public string GatewayReferenceId { get; set; } = string.Empty;
}

/*======================== 2. CreatePaymentAsync(PaymentCreateDto dto). ===========================*/
/*======================== 7. GetUserPaymentsAsync(string userId). ===========================*/
public class PaymentDto
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public PaymentStatus Status { get; set; }

    public PaymentGatewayType PaymentGateway { get; set; }
    public PaymentPurpose PaymentPurpose { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? PaidAt { get; set; }
}

/*======================== 5. RefundAsync(RefundRequestDto dto). ===========================*/
public class RefundRequestDto
{
    public int PaymentId { get; set; }

    public decimal Amount { get; set; }

    public string? Reason { get; set; }
}

public class RefundDto
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public decimal Amount { get; set; }

    public string? Reason { get; set; }

    public RefundStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

/*======================== 6. GetPaymentDetailsAsync(int paymentId). ===========================*/
public class PaymentDetailsDto : PaymentDto
{
    public string? GatewayReferenceId { get; set; }

    public string? Metadata { get; set; }

    public List<PaymentItemDto> Items { get; set; } = new();
}

/*======================== . ===========================*/
public class PaymentItemDto
{
    public PaymentItemType ItemType { get; set; }

    public int ItemId { get; set; }

    public decimal Amount { get; set; }
}

public class PaymentGatewayInitResult
{
    public string RedirectUrl { get; set; } = null!;
    public string GatewayReferenceId { get; set; } = null!;
    public Dictionary<string, string>? AdditionalData { get; set; } // optional metadata
}

public class PaymentRequestDto
{
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string UserEmail { get; set; } = default!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";

    public List<string> ItemNames { get; set; } = new();

    public string ReferenceId { get; set; } = default!;
    public string SiteUrl { get; set; } = default!;
    public string ReturnUrl { get; set; } = default!;

    public string ProfileId { get; set; } = default!;
    public string CallbackUrl { get; set; } = default!;
}

public class PaymentInitiationResult
{
    public bool IsSuccess { get; set; }
    public string? RedirectUrl { get; set; }
    public string? GatewayReferenceId { get; set; }
    public string? ErrorMessage { get; set; }

    public static PaymentInitiationResult Success(string redirectUrl, string gatewayRefId)
    {
        return new PaymentInitiationResult
        {
            IsSuccess = true,
            RedirectUrl = redirectUrl,
            GatewayReferenceId = gatewayRefId
        };
    }

    public static PaymentInitiationResult Fail(string errorMessage)
    {
        return new PaymentInitiationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

public class PayTabsV3Response
{
    public string? RedirectUrl { get; set; }

    [JsonPropertyName("tran_ref")]
    public string TransactionReference { get; set; } = string.Empty;

    [JsonPropertyName("payment_result")]
    public PayTabsPaymentResult? PaymentResult { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class PayTabsV3QueryResponse
{
    [JsonPropertyName("tran_ref")]
    public string TransactionReference { get; set; } = string.Empty;

    [JsonPropertyName("payment_result")]
    public PayTabsPaymentResult? PaymentResult { get; set; }

    [JsonPropertyName("transaction_type")]
    public string? TransactionType { get; set; }

    [JsonPropertyName("transaction_class")]
    public string? TransactionClass { get; set; }

    [JsonPropertyName("cart_id")]
    public string? CartId { get; set; }

    [JsonPropertyName("cart_currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("cart_amount")]
    public decimal Amount { get; set; }
}

public class PayTabsV3RefundResponse
{
    [JsonPropertyName("tran_ref")]
    public string TransactionReference { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class PayTabsPaymentResult
{
    [JsonPropertyName("response_status")]
    public string? ResponseStatus { get; set; }

    [JsonPropertyName("response_message")]
    public string? ResponseMessage { get; set; }

    [JsonPropertyName("response_code")]
    public string? ResponseCode { get; set; }
}

public class PaymentItemCreateDto
{
    public PaymentItemType ItemType { get; set; }

    public int ItemId { get; set; }

    public decimal Amount { get; set; }
}
public class PaymentGatewayInitiationResult
{
    public string? RedirectUrl { get; set; }
    public string? GatewayReferenceId { get; set; }
}
