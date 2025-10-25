using Lumora.DTOs.Test;

namespace Lumora.Interfaces.TestIntf
{
    public interface ITestService
    {
        /// <summary>
        /// Creates a new test with optional questions and choices.
        /// </summary>
        /// <param name="dto">The DTO containing test creation data.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Returns the ID of the newly created test.</returns>
        Task<GeneralResult<int>> CreateTestAsync(TestCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the details of an existing test.
        /// </summary>
        /// <param name="testId">The ID of the test to update.</param>
        /// <param name="dto">The DTO containing updated test data.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Returns a result indicating whether the update was successful.</returns>
        Task<GeneralResult> UpdateTestAsync(int testId, TestUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Soft deletes a test by marking it as deleted.
        /// </summary>
        /// <param name="testId">The ID of the test to delete.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Returns a result indicating whether the deletion was successful.</returns>
        Task<GeneralResult> DeleteTestAsync(int testId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the details of a specific test by its ID.
        /// </summary>
        /// <param name="testId">The ID of the test.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Returns the full details of the test including metadata.</returns>
        Task<GeneralResult<TestDetailsDto>> GetTestByIdAsync(int testId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a paginated list of tests associated with a specific lesson.
        /// </summary>
        /// <returns>Returns a paginated list of test summaries.</returns>
        Task<GeneralResult<PagedResult<TestDetailsDto>>> GetAllTestsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken);
    }
}
