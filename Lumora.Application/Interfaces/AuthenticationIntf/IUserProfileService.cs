namespace Lumora.Application.Interfaces.AuthenticationIntf
{
    public interface IUserProfileService
    {
        /// <summary>
        /// Changes the phone number for a user with the provided user ID and new phone number.
        /// </summary>
        Task<GeneralResult> ChangePhoneNumberAsync(string userId, string newPhoneNumber, CancellationToken cancellationToken);

        /// <summary>
        /// Completes user data (such as adding personal data or any missing information).
        /// </summary>
        Task<GeneralResult> CompleteProfileAsync(string userId, CompleteUserDataDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates user information.
        /// </summary>
        Task<GeneralResult> UpdateProfileAsync(string userId, UserUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the profile information for a user with the provided user ID.
        /// </summary>
        Task<GeneralResult<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken, PaginationRequestDto pagination);
    }
}
