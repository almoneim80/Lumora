using Lumora.Interfaces.PaymentIntf;

namespace Lumora.Services.PaymentSvc
{
    public class RefundRepository(PgDbContext dbContext) : IRefundRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<decimal> GetTotalRefundedAmountAsync(int paymentId)
        {
            return await _dbContext.Refunds
                .Where(r => r.PaymentId == paymentId)
                .SumAsync(r => r.Amount);
        }

        public Task AddAsync(Refund refund)
        {
            _dbContext.Refunds.Add(refund);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return _dbContext.SaveChangesAsync();
        }
    }
}
