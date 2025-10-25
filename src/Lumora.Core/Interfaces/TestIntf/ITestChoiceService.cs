using Lumora.DTOs.Test;

namespace Lumora.Interfaces.TestIntf
{
    public interface ITestChoiceService
    {
        /// <summary>
        /// Adds a new choice to a specific test question.
        /// </summary>
        /// <param name="dto">The DTO containing the text, question ID, and correctness flag.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns the ID of the newly created choice.</returns>
        Task<GeneralResult<int>> AddChoiceAsync(ChoiceCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing test choice with new data.
        /// </summary>
        /// <param name="choiceId">The ID of the choice to update.</param>
        /// <param name="dto">The DTO containing updated choice data.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns a result indicating whether the update was successful.</returns>
        Task<GeneralResult> UpdateChoiceAsync(int choiceId, ChoiceUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Soft deletes a test choice by marking it as deleted.
        /// </summary>
        /// <param name="choiceId">The ID of the choice to delete.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns a result indicating whether the deletion was successful.</returns>
        Task<GeneralResult> DeleteChoiceAsync(int choiceId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all choices associated with a specific test question.
        /// </summary>
        /// <param name="questionId">The ID of the question to retrieve choices for.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns a list of choice details.</returns>
        Task<GeneralResult<List<ChoiceDetailsDto>>> GetChoicesByQuestionIdAsync(int questionId, CancellationToken cancellationToken);

        /// <summary>
        /// Sets whether a specific choice is marked as correct or incorrect.
        /// </summary>
        /// <param name="choiceId">The ID of the choice to update.</param>
        /// <param name="isCorrect">True if the choice is correct, false otherwise.</param>
        /// <param name="cancellationToken">Token to cancel the operation if requested.</param>
        /// <returns>Returns a result indicating whether the correctness flag was updated successfully.</returns>
        Task<GeneralResult> SetChoiceCorrectnessAsync(int choiceId, bool isCorrect, CancellationToken cancellationToken);
    }
}
