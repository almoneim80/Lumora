namespace Lumora.Application.Services.TestSvc
{
    public class TestChoiceService(
            ITestChoiceRepository repository,
            ILogger<TestChoiceService> logger,
            TestMessage messages) : ITestChoiceService
    {
        private readonly ITestChoiceRepository _repository = repository;
        private readonly ILogger<TestChoiceService> _logger = logger;
        private readonly TestMessage _messages = messages;

        public async Task<GeneralResult<int>> AddChoiceAsync(ChoiceCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null) return new GeneralResult<int>(false, _messages.MsgChoiceDtoNull, 0, ErrorType.BadRequest);
                if (dto.QuestionId <= 0) return new GeneralResult<int>(false, _messages.MsgQuestionIdInvalid, 0, ErrorType.BadRequest);
                if (string.IsNullOrWhiteSpace(dto.Text)) return new GeneralResult<int>(false, _messages.MsgChoiceTextRequired, 0, ErrorType.BadRequest);

                var questionExists = await _repository.QuestionExistsAsync(dto.QuestionId, cancellationToken);
                if (!questionExists) return new GeneralResult<int>(false, _messages.MsgQuestionNotFound, 0, ErrorType.NotFound);

                if (dto.IsCorrect)
                {
                    if (await _repository.HasCorrectChoiceAsync(dto.QuestionId, null, cancellationToken))
                        return new GeneralResult<int>(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, 0, ErrorType.BadRequest);
                }

                var count = await _repository.GetChoicesCountAsync(dto.QuestionId, cancellationToken);
                var choice = new TestChoice
                {
                    TestQuestionId = dto.QuestionId,
                    Text = dto.Text.Trim(),
                    IsCorrect = dto.IsCorrect,
                    DisplayOrder = count + 1,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _repository.AddAsync(choice, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                return new GeneralResult<int>(true, _messages.MsgChoiceCreated, choice.Id, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding choice");
                return new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("Add Choice"), 0, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> UpdateChoiceAsync(int choiceId, ChoiceUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null) return new GeneralResult(false, _messages.MsgChoiceDtoNull, null, ErrorType.BadRequest);

                var choice = await _repository.GetByIdAsync(choiceId, cancellationToken);
                if (choice == null) return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.NotFound);

                if (dto.IsCorrect.HasValue && dto.IsCorrect.Value)
                {
                    if (await _repository.HasCorrectChoiceAsync(choice.TestQuestionId, choiceId, cancellationToken))
                        return new GeneralResult(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, null, ErrorType.BadRequest);
                    choice.IsCorrect = true;
                }
                else if (dto.IsCorrect.HasValue)
                {
                    choice.IsCorrect = false;
                }

                if (!string.IsNullOrWhiteSpace(dto.Text)) choice.Text = dto.Text.Trim();
                choice.UpdatedAt = DateTimeOffset.UtcNow;

                _repository.Update(choice);
                await _repository.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgChoiceUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating choice");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Update Choice"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> DeleteChoiceAsync(int choiceId, CancellationToken cancellationToken)
        {
            try
            {
                var choice = await _repository.GetByIdAsync(choiceId, cancellationToken);
                if (choice == null) return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.NotFound);

                choice.IsDeleted = true;
                choice.DeletedAt = DateTimeOffset.UtcNow;

                var remainingChoices = await _repository.GetChoicesByQuestionIdAsync(choice.TestQuestionId, cancellationToken);
                for (int i = 0; i < remainingChoices.Count; i++)
                {
                    remainingChoices[i].DisplayOrder = i + 1;
                    remainingChoices[i].UpdatedAt = DateTimeOffset.UtcNow;
                }

                await _repository.SaveChangesAsync(cancellationToken);
                return new GeneralResult(true, _messages.MsgChoiceDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting choice");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Delete Choice"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<List<ChoiceDetailsDto>>> GetChoicesByQuestionIdAsync(int questionId, CancellationToken cancellationToken)
        {
            try
            {
                if (questionId <= 0) return new GeneralResult<List<ChoiceDetailsDto>>(false, _messages.MsgQuestionIdInvalid, null, ErrorType.BadRequest);

                var exists = await _repository.QuestionExistsAsync(questionId, cancellationToken);
                if (!exists) return new GeneralResult<List<ChoiceDetailsDto>>(false, _messages.MsgQuestionNotFound, null, ErrorType.NotFound);

                var choices = await _repository.GetChoicesByQuestionIdAsync(questionId, cancellationToken);
                var dtos = choices.Select(c => new ChoiceDetailsDto
                {
                    Id = c.Id,
                    QuestionId = c.TestQuestionId,
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                }).ToList();

                return new GeneralResult<List<ChoiceDetailsDto>>(true, _messages.MsgChoicesRetrieved, dtos, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting choices");
                return new GeneralResult<List<ChoiceDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get Choices"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> SetChoiceCorrectnessAsync(int choiceId, bool isCorrect, CancellationToken cancellationToken)
        {
            try
            {
                var choice = await _repository.GetByIdAsync(choiceId, cancellationToken);
                if (choice == null) return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.NotFound);

                if (isCorrect)
                {
                    var allChoices = await _repository.GetChoicesByQuestionIdAsync(choice.TestQuestionId, cancellationToken);
                    foreach (var other in allChoices.Where(c => c.Id != choiceId && c.IsCorrect))
                    {
                        other.IsCorrect = false;
                        other.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                }

                choice.IsCorrect = isCorrect;
                choice.UpdatedAt = DateTimeOffset.UtcNow;

                await _repository.SaveChangesAsync(cancellationToken);
                return new GeneralResult(true, _messages.MsgChoiceCorrectnessUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting correctness");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Set Choice Correctness"), null, ErrorType.InternalServerError);
            }
        }
    }
}
