using Lumora.DTOs.AffiliateMarketing;

namespace Lumora.Interfaces.AffiliateMarketingIntf
{
    public interface IAffiliateService
    {
        /// <summary>
        /// Creates a new promo code for a specific training program, linked to the requesting user (affiliate),
        /// with specified discount and commission percentages. The code can be generated automatically or provided manually.
        /// Validates program existence and code uniqueness before creation.
        /// </summary>
        Task<GeneralResult> CreatePromoCodeAsync(PromoCodeCreateDto dto, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Registers the usage of a promo code based on a completed payment transaction. 
        /// Validates the existence of the payment, verifies the presence and validity of the associated promo code,
        /// and creates a usage record to track the affiliate commission and usage statistics.
        /// </summary>
        Task<GeneralResult> RegisterPromoCodeUsageAsync(int paymentId, CancellationToken cancellationToken);

        /// <summary>
        /// Deactivates all active promo codes in the system by setting their status to inactive and recording the deactivation timestamp.
        /// This operation is useful for temporarily disabling all promo codes without deleting them.
        /// </summary>
        Task<GeneralResult> DeactivateAllPromoCodesAsync(string performedByUserId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a summarized report of all promo codes, including code value, associated affiliate name,
        /// training program title, activation status, and the total number of times each code was used.
        /// This is typically used for administrative dashboards and affiliate performance tracking.
        /// </summary>
        Task<GeneralResult<List<PromoCodeReportDto>>> GetPromoCodeReportAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all promo codes created by a specific user (affiliate),
        /// including their activation status, associated program, and usage count.
        /// </summary>
        Task<GeneralResult<List<PromoCodeReportDto>>> GetPromoCodesByUserAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Reactivates a previously deactivated promo code, if valid and not deleted.
        /// Useful for restoring promo code functionality without recreating it.
        /// </summary>
        Task<GeneralResult> ReactivatePromoCodeAsync(int promoCodeId, string performedByUserId, CancellationToken cancellationToken);
    }
}
