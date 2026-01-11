namespace Lumora.Application.Services.AffiliateMarketingSvc
{
    public class AffiliateRepository(PgDbContext dbContext) : IAffiliateRepository
    {
        public async Task<User?> GetUserByIdAsync(string userId, CancellationToken ct)
            => await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);

        public async Task<TrainingProgram?> GetProgramByIdAsync(int programId, CancellationToken ct)
            => await dbContext.TrainingPrograms.AsNoTracking().FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted, ct);

        public async Task<bool> IsPromoCodeDuplicateAsync(string code, CancellationToken ct)
            => await dbContext.PromoCodes.AsNoTracking().AnyAsync(pc => pc.Code == code && !pc.IsDeleted, ct);

        public async Task AddPromoCodeAsync(PromoCode promoCode, CancellationToken ct)
            => await dbContext.PromoCodes.AddAsync(promoCode, ct);

        public async Task<Payment?> GetPaymentWithPromoCodeAsync(int paymentId, CancellationToken ct)
            => await dbContext.Payments.Include(p => p.PromoCode)
                .AsNoTracking().FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted, ct);

        public async Task<bool> IsUsageRegisteredAsync(int paymentId, CancellationToken ct)
            => await dbContext.PromoCodeUsages.AsNoTracking().AnyAsync(u => u.PaymentId == paymentId, ct);

        public async Task AddPromoCodeUsageAsync(PromoCodeUsage usage, CancellationToken ct)
            => await dbContext.PromoCodeUsages.AddAsync(usage, ct);

        public async Task<List<PromoCode>> GetActivePromoCodesAsync(CancellationToken ct)
            => await dbContext.PromoCodes.Where(pc => pc.IsActive && !pc.IsDeleted).ToListAsync(ct);

        public async Task<PromoCode?> GetPromoCodeByIdAsync(int id, CancellationToken ct)
            => await dbContext.PromoCodes.FirstOrDefaultAsync(pc => pc.Id == id && !pc.IsDeleted, ct);

        public async Task<List<PromoCodeReportDto>> GetPromoCodeReportAsync(CancellationToken ct)
        {
            return await dbContext.PromoCodes
                .Where(pc => !pc.IsDeleted)
                .Select(pc => new PromoCodeReportDto
                {
                    Code = pc.Code,
                    UserFullName = pc.User.FullName ?? "",
                    ProgramTitle = pc.TrainingProgram.Name ?? "",
                    IsActive = pc.IsActive,
                    UsageCount = dbContext.PromoCodeUsages.Count(u => u.PromoCodeId == pc.Id)
                }).AsNoTracking().ToListAsync(ct);
        }

        public async Task<List<PromoCodeReportDto>> GetPromoCodesByUserAsync(string userId, CancellationToken ct)
        {
            return await dbContext.PromoCodes
                .Where(pc => pc.UserId == userId && !pc.IsDeleted)
                .Select(pc => new PromoCodeReportDto
                {
                    Code = pc.Code,
                    UserFullName = pc.User.FullName ?? "",
                    ProgramTitle = pc.TrainingProgram.Name ?? "",
                    IsActive = pc.IsActive,
                    UsageCount = dbContext.PromoCodeUsages.Count(u => u.PromoCodeId == pc.Id)
                }).AsNoTracking().ToListAsync(ct);
        }

        public async Task UpdatePromoCodesAsync(IEnumerable<PromoCode> promoCodes, CancellationToken ct)
        {
            dbContext.PromoCodes.UpdateRange(promoCodes);
            await dbContext.SaveChangesAsync(ct);
        }

        public async Task<bool> UserExistsAsync(string userId, CancellationToken ct) =>
            await dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId && !u.IsDeleted, ct);

        public void UpdatePromoCode(PromoCode promoCode)
        {
            /* Mark the entity as modified in the change tracker */
            dbContext.PromoCodes.Update(promoCode);
        }
    }
}
