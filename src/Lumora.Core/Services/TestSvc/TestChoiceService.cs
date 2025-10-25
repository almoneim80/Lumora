using Lumora.DTOs.Test;
using Lumora.Interfaces.TestIntf;

namespace Lumora.Services.TestSvc
{
    public class TestChoiceService(PgDbContext dbContext, ILogger<TestChoiceService> logger, TestMessage messages) : ITestChoiceService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<TestChoiceService> _logger = logger;
        private readonly TestMessage _messages = messages;

        /// <inheritdoc />
        public async Task<GeneralResult<int>> AddChoiceAsync(ChoiceCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("TestChoiceService - AddChoiceAsync: DTO is null.");
                    return new GeneralResult<int>(false, _messages.MsgChoiceDtoNull, 0, ErrorType.BadRequest);
                }

                if (dto.QuestionId <= 0)
                {
                    _logger.LogWarning("TestChoiceService - AddChoiceAsync: Invalid QuestionId.");
                    return new GeneralResult<int>(false, _messages.MsgQuestionIdInvalid, 0, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(dto.Text))
                {
                    _logger.LogWarning("TestChoiceService - AddChoiceAsync: Choice text is required.");
                    return new GeneralResult<int>(false, _messages.MsgChoiceTextRequired, 0, ErrorType.BadRequest);
                }

                var questionExists = await _dbContext.TestQuestions
                    .AnyAsync(q => q.Id == dto.QuestionId && !q.IsDeleted, cancellationToken);

                if (!questionExists)
                {
                    _logger.LogWarning("TestChoiceService - AddChoiceAsync: Question {QuestionId} not found.", dto.QuestionId);
                    return new GeneralResult<int>(false, _messages.MsgQuestionNotFound, 0, ErrorType.NotFound);
                }

                if (dto.IsCorrect)
                {
                    var alreadyHasCorrect = await _dbContext.TestChoices
                        .AnyAsync(c =>
                            c.TestQuestionId == dto.QuestionId &&
                            !c.IsDeleted &&
                            c.IsCorrect,
                            cancellationToken);

                    if (alreadyHasCorrect)
                    {
                        _logger.LogWarning("TestChoiceService - AddChoiceAsync: Question {QuestionId} already has a correct choice.", dto.QuestionId);
                        return new GeneralResult<int>(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, 0, ErrorType.BadRequest);
                    }
                }

                var currentChoicesCount = await _dbContext.TestChoices
                    .CountAsync(c => c.TestQuestionId == dto.QuestionId && !c.IsDeleted, cancellationToken);

                var choice = new TestChoice
                {
                    TestQuestionId = dto.QuestionId,
                    Text = dto.Text.Trim(),
                    IsCorrect = dto.IsCorrect,
                    DisplayOrder = currentChoicesCount + 1,
                    CreatedAt = now
                };

                await _dbContext.TestChoices.AddAsync(choice, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("TestChoiceService - AddChoiceAsync: Choice added successfully to question {QuestionId}.", dto.QuestionId);
                return new GeneralResult<int>(true, _messages.MsgChoiceCreated, choice.Id, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestChoiceService - AddChoiceAsync: Error adding choice to question {QuestionId}.", dto?.QuestionId);
                return new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("Add Choice"), 0, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> UpdateChoiceAsync(int choiceId, ChoiceUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("TestChoiceService - UpdateChoiceAsync: DTO is null.");
                    return new GeneralResult(false, _messages.MsgChoiceDtoNull, null, ErrorType.BadRequest);
                }

                var choice = await _dbContext.TestChoices
                    .FirstOrDefaultAsync(c => c.Id == choiceId && !c.IsDeleted, cancellationToken);

                if (choice == null)
                {
                    _logger.LogWarning("TestChoiceService - UpdateChoiceAsync: Choice with ID {ChoiceId} not found.", choiceId);
                    return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.NotFound);
                }

                if (!string.IsNullOrWhiteSpace(dto.Text))
                    choice.Text = dto.Text.Trim();

                // If IsCorrect is set to true, check if another choice is already correct
                if (dto.IsCorrect.HasValue && dto.IsCorrect.Value)
                {
                    var hasAnotherCorrect = await _dbContext.TestChoices
                        .AnyAsync(c =>
                            c.TestQuestionId == choice.TestQuestionId &&
                            c.Id != choiceId &&
                            !c.IsDeleted &&
                            c.IsCorrect,
                            cancellationToken);

                    if (hasAnotherCorrect)
                    {
                        _logger.LogWarning("TestChoiceService - UpdateChoiceAsync: Another correct choice already exists for question {QuestionId}.", choice.TestQuestionId);
                        return new GeneralResult(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, null, ErrorType.BadRequest);
                    }

                    choice.IsCorrect = true;
                }
                else if (dto.IsCorrect.HasValue)
                {
                    choice.IsCorrect = false;
                }

                if (dto.IsCorrect.HasValue)
                    choice.IsCorrect = dto.IsCorrect.Value;

                choice.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("TestChoiceService - UpdateChoiceAsync: Choice {ChoiceId} updated successfully.", choiceId);

                return new GeneralResult(true, _messages.MsgChoiceUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestChoiceService - UpdateChoiceAsync: Error updating choice {ChoiceId}.", choiceId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Update Choice"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> DeleteChoiceAsync(int choiceId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                var choice = await _dbContext.TestChoices
                    .FirstOrDefaultAsync(c => c.Id == choiceId && !c.IsDeleted, cancellationToken);

                if (choice == null)
                {
                    _logger.LogWarning("TestChoiceService - DeleteChoiceAsync: Choice with ID {ChoiceId} not found.", choiceId);
                    return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.NotFound);
                }

                choice.IsDeleted = true;
                choice.DeletedAt = now;

                // Re-sequence DisplayOrder for remaining choices
                var remainingChoices = await _dbContext.TestChoices
                    .Where(c => c.TestQuestionId == choice.TestQuestionId && !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync(cancellationToken);

                for (int i = 0; i < remainingChoices.Count; i++)
                {
                    remainingChoices[i].DisplayOrder = i + 1;
                    remainingChoices[i].UpdatedAt = DateTimeOffset.UtcNow;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("TestChoiceService - DeleteChoiceAsync: Choice {ChoiceId} deleted successfully.", choiceId);

                return new GeneralResult(true, _messages.MsgChoiceDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestChoiceService - DeleteChoiceAsync: Error deleting choice {ChoiceId}.", choiceId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Delete Choice"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<List<ChoiceDetailsDto>>> GetChoicesByQuestionIdAsync(int questionId, CancellationToken cancellationToken)
        {
            try
            {
                if (questionId <= 0)
                {
                    _logger.LogWarning("TestChoiceService - GetChoicesByQuestionIdAsync: Invalid question ID {QuestionId}.", questionId);
                    return new GeneralResult<List<ChoiceDetailsDto>>(false, _messages.MsgQuestionIdInvalid, null, ErrorType.BadRequest);
                }

                var questionExists = await _dbContext.TestQuestions
                    .AnyAsync(q => q.Id == questionId && !q.IsDeleted, cancellationToken);

                if (!questionExists)
                {
                    _logger.LogWarning("TestChoiceService - GetChoicesByQuestionIdAsync: Question with ID {QuestionId} not found.", questionId);
                    return new GeneralResult<List<ChoiceDetailsDto>>(false, _messages.MsgQuestionNotFound, null, ErrorType.NotFound);
                }

                var choices = await _dbContext.TestChoices
                    .AsNoTracking()
                    .Where(c => c.TestQuestionId == questionId && !c.IsDeleted)
                    .OrderByDescending(c => c.DisplayOrder)
                    .Select(c => new ChoiceDetailsDto
                    {
                        Id = c.Id,
                        QuestionId = c.TestQuestionId,
                        Text = c.Text,
                        IsCorrect = c.IsCorrect
                    })
                    .ToListAsync(cancellationToken);

                if (!choices.Any())
                {
                    _logger.LogInformation("TestChoiceService - GetChoicesByQuestionIdAsync: No choices found for question {QuestionId}.", questionId);
                    return new GeneralResult<List<ChoiceDetailsDto>>(false, _messages.MsgChoicesNotFound, null, ErrorType.NotFound);
                }

                _logger.LogInformation("TestChoiceService - GetChoicesByQuestionIdAsync: Retrieved {Count} choices for question {QuestionId}.", choices.Count, questionId);
                return new GeneralResult<List<ChoiceDetailsDto>>(true, _messages.MsgChoicesRetrieved, choices, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestChoiceService - GetChoicesByQuestionIdAsync: Error retrieving choices for question {QuestionId}.", questionId);
                return new GeneralResult<List<ChoiceDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get Choices"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> SetChoiceCorrectnessAsync(int choiceId, bool isCorrect, CancellationToken cancellationToken)
        {
            try
            {
                if (choiceId <= 0)
                {
                    _logger.LogWarning("TestChoiceService - SetChoiceCorrectnessAsync: Invalid choice ID {ChoiceId}.", choiceId);
                    return new GeneralResult(false, _messages.MsgChoiceIdInvalid, null, ErrorType.BadRequest);
                }

                var choice = await _dbContext.TestChoices
                    .FirstOrDefaultAsync(c => c.Id == choiceId && !c.IsDeleted, cancellationToken);

                if (choice == null)
                {
                    _logger.LogWarning("TestChoiceService - SetChoiceCorrectnessAsync: Choice with ID {ChoiceId} not found.", choiceId);
                    return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.NotFound);
                }

                if (isCorrect)
                {
                    var otherChoices = await _dbContext.TestChoices
                        .Where(c => c.TestQuestionId == choice.TestQuestionId && c.Id != choiceId && !c.IsDeleted)
                        .ToListAsync(cancellationToken);

                    foreach (var other in otherChoices)
                    {
                        if (other.IsCorrect)
                        {
                            other.IsCorrect = false;
                            other.UpdatedAt = DateTimeOffset.UtcNow;
                        }
                    }
                }

                choice.IsCorrect = isCorrect;
                choice.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("TestChoiceService - SetChoiceCorrectnessAsync: Choice {ChoiceId} updated to IsCorrect = {IsCorrect}.", choiceId, isCorrect);
                return new GeneralResult(true, _messages.MsgChoiceCorrectnessUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestChoiceService - SetChoiceCorrectnessAsync: Error updating correctness for choice {ChoiceId}.", choiceId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Set Choice Correctness"), null, ErrorType.InternalServerError);
            }
        }
    }
}
