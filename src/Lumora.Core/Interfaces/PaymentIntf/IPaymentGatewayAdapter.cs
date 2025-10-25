using Lumora.DTOs.Payment;

namespace Lumora.Interfaces.PaymentIntf
{
    public interface IPaymentGatewayAdapter
    {
        Task<GeneralResult<PaymentGatewayInitiationResult>> InitiateAsync(PaymentRequestDto dto);
        Task<GeneralResult<PaymentStatus>> CheckStatusAsync(string gatewayReferenceId);
        Task<GeneralResult> RefundAsync(string gatewayReferenceId, decimal amount);
    }
}
