using Lumora.DTOs.Club;

namespace Lumora.Interfaces.ClubIntf
{
    public interface IWheelPlayerService
    {
        /// <summary>
        /// Checks if the user has played today or not.
        /// </summary>
        Task<GeneralResult> CanPlayTodayAsync(string playerId);

        /// <summary>
        ///  This method implements the logic of spinning the wheel, selecting the prize, and recording the result.
        /// </summary>
        Task<GeneralResult> SpinAsync(string playerId, int awardId);

        /// <summary>
        /// Displays the history of all previous attempts for this player.
        /// </summary>
        Task<GeneralResult> GetPlayerHistoryAsync(string playerId);

        /// <summary>
        /// Displays the details of today's attempt, if any (no new rotation attempt).
        /// </summary>
        Task<GeneralResult> GetTodaySpinAsync(string playerId);

        /// <summary>
        /// Ensures today's spin state record exists for the player.
        /// Creates or resets daily state as needed.
        /// </summary>
        Task EnsurePlayerSpinStateAsync(string playerId);

        /// <summary>
        /// Activates the player's paid spin.
        /// </summary>
        Task ActivatePaidSpinAsync(string playerId);

        /// <summary>
        /// Marks a player's spin as delivered.
        /// </summary>
        Task<GeneralResult> MarkPlayDeliveredAsync(int playId, bool isDelivered);

        /// <summary>
        /// Retrieves a list of all plays for the specified delivery status.
        /// </summary>  
        Task<GeneralResult<PagedResult<WheelPlayDto>>> GetPlaysByDeliveryStatusAsync(bool delivered, PaginationRequestDto pagination, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of all plays for the specified player.
        /// </summary>
        Task<GeneralResult<PagedResult<WheelPlayDto>>> GetAllUserPlaysAsync(PaginationRequestDto pagination, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of all physical item wheel plays across all users,
        /// optionally filtered by delivery status.
        /// </summary>
        Task<GeneralResult<PagedResult<WheelPlayDto>>> GetPhysicalItemPlaysByDeliveryStatusAsync(bool? isDelivered, PaginationRequestDto pagination, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the delivery status of a physical item award for a specific wheel play entry.
        /// </summary>
        Task<GeneralResult> UpdatePhysicalItemDeliveryStatusAsync(int playId, bool isDelivered, CancellationToken cancellationToken = default);
    }
}
