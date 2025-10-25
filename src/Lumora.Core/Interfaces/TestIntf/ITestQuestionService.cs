using Lumora.DTOs.Test;

namespace Lumora.Interfaces.TestIntf
{
    public interface ITestQuestionService
    {
        /// <summary>
        /// Adds a new question to a specified test along with optional choices.
        /// </summary>
        /// <param name="dto">DTO containing the question content and associated metadata.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns the ID of the newly created question.</returns>
        Task<GeneralResult<int>> AddQuestionAsync(QuestionWithChoiseCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing test question with new content and/or metadata.
        /// </summary>
        /// <param name="questionId">The ID of the question to update.</param>
        /// <param name="dto">DTO containing the updated content of the question.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns a result indicating the success or failure of the update.</returns>
        Task<GeneralResult> UpdateQuestionAsync(int questionId, DTOs.Test.TestQuestionUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Soft deletes a question and all of its related choices and answers.
        /// </summary>
        /// <param name="questionId">The ID of the question to delete.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns a result indicating whether the deletion was successful.</returns>
        Task<GeneralResult> DeleteQuestionAsync(int questionId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves full details of a specific question including its choices.
        /// </summary>
        /// <param name="questionId">The ID of the question to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns the question with its content and associated choices.</returns>
        Task<GeneralResult<DTOs.Test.QuestionDetailsDto>> GetQuestionByIdAsync(int questionId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a paginated list of questions for a specific test.
        /// </summary>
        /// <param name="testId">The ID of the test whose questions are to be retrieved.</param>
        /// <param name="pagination">Pagination parameters to control the result set.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns a paginated list of question details.</returns>
        Task<GeneralResult<PagedResult<DTOs.Test.QuestionDetailsDto>>> GetQuestionsByTestIdAsync(int testId, PaginationRequestDto pagination, CancellationToken cancellationToken);

        /// <summary>
        /// Reorders the questions of a test by updating their display order.
        /// </summary>
        /// <param name="testId">The ID of the test whose questions will be reordered.</param>
        /// <param name="reorderList">List of question IDs with their new display orders.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns a result indicating whether the reordering was successful.</returns>
        Task<GeneralResult> ReorderQuestionsAsync(int testId, List<ReorderDto> reorderList, CancellationToken cancellationToken);
    }
}
