using System.Net.Http.Headers;
using Lumora.DTOs.Payment;
using Lumora.Interfaces.PaymentIntf;

namespace Lumora.Services.PaymentSvc.Gateways
{
    public class PayTabsAdapter(HttpClient httpClient, IConfiguration configuration, ILogger<PayTabsAdapter> logger) : IPaymentGatewayAdapter
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<PayTabsAdapter> _logger = logger;
        private readonly string _serverKey = configuration["PayTabs:ServerKey"]!;
        private readonly string _baseUrl = configuration["PayTabs:BaseUrl"] ?? "https://secure.paytabs.com/";

        private void SetAuthHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serverKey);
        }

        public async Task<GeneralResult<PaymentGatewayInitiationResult>> InitiateAsync(PaymentRequestDto dto)
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(configuration["PayTabs:BaseUrl"]!);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration["PayTabs:ServerKey"]}");

                var payload = new
                {
                    profile_id = int.Parse(dto.ProfileId),
                    tran_type = "sale",
                    tran_class = "ecom",
                    cart_id = dto.ReferenceId,
                    cart_currency = dto.Currency,
                    cart_amount = dto.Amount,
                    cart_description = "Lumora Payment",
                    customer_details = new
                    {
                        name = dto.UserName,
                        email = dto.UserEmail
                    },
                    return_url = dto.ReturnUrl,
                    callback_url = dto.CallbackUrl
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("payment/request", content);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogWarning("PayTabsAdapter - Raw response from PayTabs: {Raw}", responseString);

                // تحقق من الاستجابة
                var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("code", out var codeProp) && codeProp.GetInt32() == 401)
                {
                    return new GeneralResult<PaymentGatewayInitiationResult>(
                        false, "Authentication failed. Please check your PayTabs credentials.", null, ErrorType.Unauthorized);
                }

                if (root.TryGetProperty("redirect_url", out var redirectUrlProp) &&
                    root.TryGetProperty("tran_ref", out var tranRefProp))
                {
                    var result = new PaymentGatewayInitiationResult
                    {
                        RedirectUrl = redirectUrlProp.GetString(),
                        GatewayReferenceId = tranRefProp.GetString()
                    };

                    return new GeneralResult<PaymentGatewayInitiationResult>(
                        true, "Payment initiated successfully.", result);
                }

                return new GeneralResult<PaymentGatewayInitiationResult>(
                    false, "Unknown error occurred during PayTabs initiation.", null, ErrorType.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayTabsAdapter - Exception during payment initiation");
                return new GeneralResult<PaymentGatewayInitiationResult>(
                    false, "Unexpected error while initiating PayTabs payment.", null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<PaymentStatus>> CheckStatusAsync(string gatewayReferenceId)
        {
            try
            {
                SetAuthHeader();

                var url = $"{_baseUrl}payment/query?tran_ref={gatewayReferenceId}";
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<PayTabsV3QueryResponse>(content);

                var stat = result?.PaymentResult?.ResponseStatus == "A"
                    ? PaymentStatus.Paid
                    : PaymentStatus.Failed;

                return new GeneralResult<PaymentStatus>(
                    true,
                    "paytabs gateway initiated successfully.",
                    stat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayTabsAdapter - Error in CheckStatus");
                return new GeneralResult<PaymentStatus>(
                    false,
                    "paytabs gateway not initiated.",
                    PaymentStatus.Failed);
            }
        }

        public async Task<GeneralResult> RefundAsync(string gatewayReferenceId, decimal amount)
        {
            try
            {
                SetAuthHeader();

                var request = new
                {
                    tran_ref = gatewayReferenceId,
                    refund_amount = amount,
                    refund_reason = "Customer request"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl + "payment/refund", content);
                var body = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<PayTabsV3RefundResponse>(body);

                return result?.Code == "000" // check docs for real success code
                    ? new GeneralResult(true, "Gateway refund succeeded", null, ErrorType.Success)
                    : new GeneralResult(false, "Gateway refund failed", null, ErrorType.Failed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayTabsAdapter - Refund failed");
                return new GeneralResult(false, "Gateway refund failed", null, ErrorType.InternalServerError);
            }
        }
    }
}
