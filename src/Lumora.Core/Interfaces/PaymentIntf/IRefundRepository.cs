namespace Lumora.Interfaces.PaymentIntf
{
    public interface IRefundRepository
    {
        Task<decimal> GetTotalRefundedAmountAsync(int paymentId);
        Task AddAsync(Refund refund);
        Task SaveChangesAsync();
    }
}
