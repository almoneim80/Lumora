using Lumora.DTOs.Payment;
namespace Lumora.Interfaces.PaymentIntf
{
    public interface IPaymentService
    {
        /// <summary>
        /// Start payment process (e.g., redirect to payment gateway).
        /// </summary>
        Task<GeneralResult<PaymentStartResultDto>> StartPaymentAsync(PaymentCreateDto dto);

        /// <summary>
        /// Create a new batch (whatever type: course, program, etc.)
        /// </summary>
        Task<GeneralResult<PaymentDto>> CreatePaymentAsync(PaymentCreateDto dto);

        /// <summary>
        /// Check payment status (usually used in Callback/IPN).
        /// </summary>
        Task<GeneralResult<PaymentStatus>> VerifyPaymentStatusAsync(string gatewayReferenceId);

        /// <summary>
        /// Link payment to targeted items (e.g., activate access to the course after payment).
        /// </summary>
        Task<GeneralResult> LinkPaymentToItemsAsync(int paymentId);

        /// <summary>
        /// Refund of a partial or full payment.
        /// </summary>
        Task<GeneralResult<RefundDto>> RefundAsync(RefundRequestDto dto);

        /// <summary>
        /// Get payment details.
        /// </summary>
        Task<GeneralResult<PaymentDetailsDto>> GetPaymentDetailsAsync(int paymentId);

        /// <summary>
        /// Get all payments for a specific user.
        /// </summary>
        Task<GeneralResult<List<PaymentDto>>> GetUserPaymentsAsync(string userId);

        /// <summary>
        /// Cancel a payment that has not yet been made (e.g., if the user has not completed the payment).
        /// </summary>
        Task<GeneralResult> CancelPendingPaymentAsync(int paymentId);

        /// <summary>
        /// Was payment for this item successful.
        /// </summary>
        Task<GeneralResult<bool>> HasUserPaidForAsync(string userId, PaymentItemType itemType, int itemId);
    }
}
