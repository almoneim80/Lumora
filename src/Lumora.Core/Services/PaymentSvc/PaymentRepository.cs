using Lumora.Interfaces.PaymentIntf;

namespace Lumora.Services.PaymentSvc
{
    public class PaymentRepository(PgDbContext dbContext) : IPaymentRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<Payment?> GetByIdAsync(int id, bool includeItems = false)
        {
            if (includeItems)
            {
                return await _dbContext.Payments
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            }

            return await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Payment?> GetByGatewayReferenceAsync(string gatewayReferenceId)
        {
            return await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.GatewayReferenceId == gatewayReferenceId && !p.IsDeleted);
        }

        public async Task<List<Payment>> GetUserPaymentsAsync(string userId)
        {
            return await _dbContext.Payments
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UserHasPaidForAsync(string userId, PaymentItemType itemType, int itemId)
        {
            return await _dbContext.Payments
                .Where(p => p.UserId == userId && p.Status == PaymentStatus.Paid && !p.IsDeleted)
                .SelectMany(p => p.Items)
                .AnyAsync(i => i.ItemType == itemType && i.ItemId == itemId);
        }

        public async Task<bool> IsDuplicatePendingPaymentAsync(string userId, PaymentItemType itemType, int itemId)
        {
            return await _dbContext.Payments
                .Where(p => p.UserId == userId && p.Status == PaymentStatus.Pending && !p.IsDeleted)
                .SelectMany(p => p.Items)
                .AnyAsync(i => i.ItemType == itemType && i.ItemId == itemId);
        }

        public Task AddAsync(Payment payment)
        {
            _dbContext.Payments.Add(payment);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return _dbContext.SaveChangesAsync();
        }
    }
}
