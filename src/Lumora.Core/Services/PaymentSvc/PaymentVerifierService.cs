using Lumora.Interfaces.PaymentIntf;
namespace Lumora.Services.PaymentSvc
{
    public class PaymentVerifierService(IPaymentRepository paymentRepository) : IPaymentVerifier
    {
        public async Task<bool> HasUserPaidForAsync(string userId, PaymentItemType itemType, int itemId)
        {
            var result = await paymentRepository.UserHasPaidForAsync(userId, itemType, itemId);
            return result;
        }
    }
}
