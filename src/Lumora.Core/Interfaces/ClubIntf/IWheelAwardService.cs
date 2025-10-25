using Lumora.DTOs.Club;

namespace Lumora.Interfaces.ClubIntf
{
    public interface IWheelAwardService
    {
        /// <summary>
        /// Retrieves a specific wheel award by its unique identifier.
        /// </summary>
        Task<GeneralResult<WheelAwardDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all wheel awards from the system.
        /// </summary>
        Task<GeneralResult<PagedResult<WheelAwardDetailsDto>>> GetAllAsync(PaginationRequestDto pagination, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new wheel award using the provided data.
        /// </summary>
        Task<GeneralResult> CreateAsync(WheelAwardCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing wheel award by its unique identifier.
        /// </summary>
        Task<GeneralResult> UpdateAsync(int id, WheelAwardUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a wheel award by its unique identifier.
        /// </summary>
        Task<GeneralResult> DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
