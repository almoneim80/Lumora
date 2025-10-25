using Lumora.DTOs.Club;
namespace Lumora.Interfaces.ClubIntf
{
    public interface IAmbassadorService
    {
        /// <summary>
        /// Assigns a user as the tourism club ambassador for a specified period.
        /// </summary>
        /// <param name="dto">The assignment data including user ID, duration, optional reason, and post ID.</param>
        /// <param name="adminId">The ID of the admin performing the assignment.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// General result containing the ID of the newly assigned ambassador, or an error.
        /// </returns>
        Task<GeneralResult> AssignAmbassadorAsync(AmbassadorAssignDto dto, string adminId, CancellationToken cancellationToken);

        /// <summary>
        /// Removes a specific ambassador record by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the ambassador assignment to remove.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// General result indicating success or failure of the removal.
        /// </returns>
        Task<GeneralResult> RemoveAmbassadorAsync(int id, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the currently active tourism club ambassador based on the current date.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// General result containing the current ambassador's post details if assigned.
        /// </returns>
        Task<GeneralResult<AmbassadorDetailsDto?>> GetCurrentAmbassadorAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a list of all previous tourism club ambassadors in chronological order.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// General result containing a list of ambassador post details for all past assignments.
        /// </returns>
        Task<GeneralResult<List<AmbassadorDetailsDto>>> GetAmbassadorHistoryAsync(CancellationToken cancellationToken);
    }
}
