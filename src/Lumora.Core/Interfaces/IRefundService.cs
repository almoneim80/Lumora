namespace Lumora.Interfaces;

public interface IRefundService
{
    /// <summary>
    /// Create a new Refund request in the database.
    /// </summary>
    Task<RefundDetailsDto> CreateRefundAsync(RefundCreateDto dto);

    /// <summary>
    /// Changing status to Approved without execution (if you have a manual approval system).
    /// </summary>
    Task<RefundDetailsDto> ApproveRefundAsync(int refundId);

    /// <summary>
    /// Deny the refund request.
    /// </summary>
    Task<RefundDetailsDto> RejectRefundAsync(int refundId);

    /// <summary>
    /// Retrieve specific recovery details.
    /// </summary>
    Task<RefundDetailsDto> GetRefundByIdAsync(int refundId);

    /// <summary>
    /// Get All Refunds requests.
    /// </summary>
    Task<IEnumerable<RefundDetailsDto>> GetAllRefundsAsync();
}
