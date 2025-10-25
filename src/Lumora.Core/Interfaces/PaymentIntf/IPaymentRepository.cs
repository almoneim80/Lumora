namespace Lumora.Interfaces.PaymentIntf
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id, bool includeItems = false);
        Task<Payment?> GetByGatewayReferenceAsync(string gatewayReferenceId);
        Task<List<Payment>> GetUserPaymentsAsync(string userId);
        Task<bool> UserHasPaidForAsync(string userId, PaymentItemType itemType, int itemId);
        Task<bool> IsDuplicatePendingPaymentAsync(string userId, PaymentItemType itemType, int itemId);
        Task AddAsync(Payment payment);
        Task SaveChangesAsync();
    }
}
