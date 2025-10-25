namespace Lumora.Interfaces.ProgramIntf
{
    public interface IEnrollmentService
    {
        /// <summary>
        /// Enroll a user in a program.
        /// </summary>
        Task<GeneralResult> EnrollInProgramAsync(int programId, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Unenroll a user from a program.
        /// </summary>
        Task<GeneralResult> UnenrollFromProgramAsync(string userId, int programId, CancellationToken cancellationToken);

        /// <summary>
        /// Get enrolled users in a program.
        /// </summary>
        Task<GeneralResult<List<EnrollmentWithUserData>>> GetEnrolledUsersAsync(int programId, CancellationToken cancellationToken);

        /// <summary>
        /// Check if a user is enrolled in a program.
        /// </summary>
        Task<GeneralResult<bool>> IsUserEnrolledAsync(string userId, int programId, CancellationToken cancellationToken);

        /// <summary>
        /// Update the status of an enrollment.
        /// </summary>
        Task<GeneralResult> UpdateEnrollmentStatusAsync(string userId, int programId, EnrollmentStatus status, CancellationToken cancellationToken);

        /// <summary>
        /// Get user enrollment info.
        /// </summary>
        Task<GeneralResult<EnrollmentWithUserData>> GetUserEnrollmentInfoAsync(string userId, int programId, CancellationToken cancellationToken);
    }
}
