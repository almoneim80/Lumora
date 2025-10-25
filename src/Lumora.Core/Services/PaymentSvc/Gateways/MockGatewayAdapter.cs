using Lumora.DTOs.Payment;
using Lumora.Interfaces.PaymentIntf;
namespace Lumora.Services.PaymentSvc.Gateways
{
    public class MockGatewayAdapter : IPaymentGatewayAdapter
    {
        public Task<GeneralResult<PaymentGatewayInitResult>> InitiateAsync(PaymentRequestDto payment)
        {
            // توليد ID وهمي للمحاكاة
            var gatewayRef = $"MOCK-{Guid.NewGuid().ToString("N").ToUpper()}";

            var result = new PaymentGatewayInitResult
            {
                RedirectUrl = $"https://mockgateway.com/pay?ref={gatewayRef}",
                GatewayReferenceId = gatewayRef,
                AdditionalData = new Dictionary<string, string>
                {
                    { "note", "This is a mock gateway simulation." }
                }
            };

            return Task.FromResult(new GeneralResult<PaymentGatewayInitResult>(
                true,
                "Mock gateway initiated successfully.",
                result));
        }

        public Task<GeneralResult<PaymentStatus>> CheckStatusAsync(string gatewayReferenceId)
        {
            // محاكاة الاستعلام: أي طلب نعتبره تم دفعه بنجاح
            var simulatedStatus = PaymentStatus.Paid;

            return Task.FromResult(new GeneralResult<PaymentStatus>(
                true,
                $"Mock status retrieved for ref {gatewayReferenceId}.",
                simulatedStatus));
        }

        public Task<GeneralResult> RefundAsync(string gatewayReferenceId, decimal amount)
        {
            // أي استرداد نعتبره ناجح دائمًا
            return Task.FromResult(new GeneralResult(
                true,
                $"Mock refund of {amount} succeeded for ref {gatewayReferenceId}."));
        }

        Task<GeneralResult<PaymentGatewayInitiationResult>> IPaymentGatewayAdapter.InitiateAsync(PaymentRequestDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
