namespace Lumora.Application.Interfaces.AffiliateMarketingIntf
{
    public interface IAffiliateRepository
    {
        Task<User?> GetUserByIdAsync(string userId, CancellationToken ct);
        Task<TrainingProgram?> GetProgramByIdAsync(int programId, CancellationToken ct);
        Task<bool> IsPromoCodeDuplicateAsync(string code, CancellationToken ct);
        Task AddPromoCodeAsync(PromoCode promoCode, CancellationToken ct);

        Task<Payment?> GetPaymentWithPromoCodeAsync(int paymentId, CancellationToken ct);
        Task<bool> IsUsageRegisteredAsync(int paymentId, CancellationToken ct);
        Task AddPromoCodeUsageAsync(PromoCodeUsage usage, CancellationToken ct);

        Task<List<PromoCode>> GetActivePromoCodesAsync(CancellationToken ct);
        Task<PromoCode?> GetPromoCodeByIdAsync(int id, CancellationToken ct);

        // Reports
        Task<List<PromoCodeReportDto>> GetPromoCodeReportAsync(CancellationToken ct);
        Task<List<PromoCodeReportDto>> GetPromoCodesByUserAsync(string userId, CancellationToken ct);
        Task UpdatePromoCodesAsync(IEnumerable<PromoCode> promoCodes, CancellationToken ct);
        Task<bool> UserExistsAsync(string userId, CancellationToken ct);
        void UpdatePromoCode(PromoCode promoCode);
    }
}
