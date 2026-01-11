namespace Lumora.Application.Interfaces.PaymentIntf
{
    public interface IPaymentVerifier
    {
        Task<bool> HasUserPaidForAsync(string userId, PaymentItemType itemType, int itemId);
    }
}
