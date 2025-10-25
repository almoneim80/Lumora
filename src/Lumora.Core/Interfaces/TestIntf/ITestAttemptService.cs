using Lumora.DTOs.Test;
namespace Lumora.Interfaces.TestIntf
{
    public interface ITestAttemptService
    {
        /// <summary>
        /// Starts a new attempt for the specified user and test.
        /// </summary>
        /// <param name="userId">The ID of the user starting the attempt.</param>
        /// <param name="testId">The ID of the test to attempt.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Returns details of the started attempt.</returns>
        Task<GeneralResult<TestAttemptStartDto>> StartAttemptAsync(string userId, int testId, CancellationToken cancellationToken);

        /// <summary>
        /// Submits a user's answer for a specific question during an attempt.
        /// </summary>
        /// <param name="dto">The submitted answer details including attempt, question, and selected choice.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Returns a result indicating whether the answer was submitted successfully.</returns>
        Task<GeneralResult> SubmitAnswerAsync(TestAnswerDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Submits the user's entire attempt and calculates the final score.
        /// </summary>
        /// <param name="attemptId">The ID of the attempt to submit.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Returns a result indicating whether the submission was successful.</returns>
        Task<GeneralResult> SubmitAttemptAsync(int attemptId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the best attempt result for a specific user and test.
        /// </summary>
        Task<GeneralResult<TestAttemptResultDto>> GetBestAttemptResultAsync(string userId, int testId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a paginated list of all attempts made by the specified user for a given test.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="testId">The ID of the test.</param>
        /// <param name="pagination">Pagination parameters.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Returns a paginated list of attempt summaries.</returns>
        Task<GeneralResult<PagedResult<TestAttemptSummaryDto>>> GetUserAttemptsAsync(string userId, int testId, PaginationRequestDto pagination, CancellationToken cancellationToken);
    }
}
