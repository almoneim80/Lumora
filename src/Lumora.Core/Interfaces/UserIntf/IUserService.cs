using Lumora.DTOs.Authentication;

namespace Lumora.Interfaces.Customer
{
    public interface IUserService
    {
        /// <summary>
        /// Checks if there is a user with this number.
        /// </summary>
        Task<GeneralResult<User>> FindUserAsync(CancellationToken cancellationToken, string? email = null, string? phoneNumber = null, string? userId = null, bool requirePhoneConfirmed = true, bool isAdmin = false);

        /// <summary>
        /// Checks if there is a user with this number.
        /// </summary>
        Task<GeneralResult<User>> FindUserWithoutPhoneNumberConfirmedAsync(CancellationToken cancellationToken, string? phoneNumber = null, string? userId = null);

        /// <summary>
        /// find the user without active validation.
        /// </summary>
        Task<GeneralResult<User>> GetUserByIdWithoutActiveValidation(string userId);

        /// <summary>
        /// Checks if there is a user with this number.
        /// </summary>
        Task<bool> ExsistByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Checks if there is a user with this Id.
        /// </summary>
        Task<bool> ExsistByIdAsync(string userId);

        /// <summary>
        /// Get all users in the system.
        /// </summary>
        Task<GeneralResult<PagedResult<ListUsersDto>>> GetAllUsersAsync(PaginationRequestDto pagination, CancellationToken cancellationToken);

        /// <summary>
        /// Get all users in the system based on activation status.
        /// </summary>
        Task<GeneralResult<PagedResult<ListUsersDto>>> GetUsersBasedOnActivationStatus(
            PaginationRequestDto pagination, CancellationToken cancellationToken, bool isActive);

        /// <summary>
        /// Find user by email address.
        /// </summary>
        Task<User> FindOnRegister(string email);
    }
}
