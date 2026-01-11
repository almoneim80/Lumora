namespace Lumora.Application.Services.TestSvc
{
    public class TestQuestionService(
        ITestQuestionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<TestQuestionService> logger,
        TestMessage messages) : ITestQuestionService
    {
        private readonly ITestQuestionRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<TestQuestionService> _logger = logger;
        private readonly TestMessage _messages = messages;

        /// <inheritdoc />
        public async Task<GeneralResult<int>> AddQuestionAsync(QuestionWithChoiseCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                // Validation logic remains in Service
                if (dto == null) return new(false, _messages.MsgQuestionDtoNull, 0, ErrorType.BadRequest);
                if (dto.TestId <= 0) return new(false, _messages.MsgTestIdInvalid, 0, ErrorType.BadRequest);
                if (string.IsNullOrWhiteSpace(dto.Question)) return new(false, _messages.MsgQuestionTextRequired, 0, ErrorType.BadRequest);

                if (dto.Choices == null || dto.Choices.Count(c => c.IsCorrect) != 1)
                    return new(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, 0, ErrorType.BadRequest);

                if (!await _repository.TestExistsAsync(dto.TestId, cancellationToken))
                    return new(false, _messages.MsgTestNotFound, 0, ErrorType.NotFound);

                var existingCount = await _repository.GetQuestionsCountAsync(dto.TestId, cancellationToken);

                var question = new TestQuestion
                {
                    TestId = dto.TestId,
                    QuestionText = dto.Question.Trim(),
                    Mark = dto.Mark,
                    DisplayOrder = existingCount + 1,
                    CreatedAt = now
                };

                await _repository.AddQuestionAsync(question, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                for (int i = 0; i < dto.Choices.Count; i++)
                {
                    await _repository.AddChoiceAsync(new TestChoice
                    {
                        TestQuestionId = question.Id,
                        Text = dto.Choices[i].Text.Trim(),
                        IsCorrect = dto.Choices[i].IsCorrect,
                        DisplayOrder = i + 1,
                        CreatedAt = now
                    }, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new(true, _messages.MsgQuestionCreated, question.Id, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding question");
                return new(false, _messages.GetUnexpectedErrorMessage("Add Question"), 0, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> UpdateQuestionAsync(int questionId, TestQuestionUpdateDto dto, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;
                var question = await _repository.GetByIdWithChoicesAsync(questionId, cancellationToken);

                if (question == null) return new(false, _messages.MsgQuestionNotFound, null, ErrorType.NotFound);

                if (!string.IsNullOrWhiteSpace(dto.Question)) question.QuestionText = dto.Question.Trim();
                if (dto.Mark.HasValue && dto.Mark > 0) question.Mark = dto.Mark.Value;

                foreach (var cDto in dto.Choices ?? [])
                {
                    if (cDto.Id.HasValue)
                    {
                        var existing = await _repository.GetChoiceByIdAsync(cDto.Id.Value, cancellationToken);
                        if (existing != null)
                        {
                            if (!string.IsNullOrWhiteSpace(cDto.Text)) existing.Text = cDto.Text.Trim();
                            if (cDto.IsCorrect.HasValue) existing.IsCorrect = cDto.IsCorrect.Value;
                        }
                    }
                    else
                    {
                        var maxOrder = question.Choices.Where(c => !c.IsDeleted).Select(c => (int?)c.DisplayOrder).Max() ?? 0;
                        await _repository.AddChoiceAsync(new TestChoice
                        {
                            TestQuestionId = questionId,
                            Text = cDto.Text?.Trim() ?? "",
                            IsCorrect = cDto.IsCorrect ?? false,
                            DisplayOrder = maxOrder + 1,
                            CreatedAt = now
                        }, cancellationToken);
                    }
                }

                if (question.Choices.Count(c => !c.IsDeleted && c.IsCorrect) != 1)
                    return new(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, null, ErrorType.BadRequest);

                var test = await _repository.GetTestWithQuestionsAsync(question.TestId, cancellationToken);
                if (test != null)
                {
                    test.TotalMark = test.Questions.Where(q => !q.IsDeleted).Sum(q => q.Mark);
                    test.UpdatedAt = now;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new(true, _messages.MsgQuestionUpdated, null, ErrorType.Success);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new(false, _messages.GetUnexpectedErrorMessage("Update Question"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> DeleteQuestionAsync(int questionId, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;
                var question = await _repository.GetByIdWithChoicesAndTestAsync(questionId, cancellationToken);

                if (question == null)
                {
                    _logger.LogWarning("TestQuestionService - DeleteQuestionAsync : Question with ID {QuestionId} not found.", questionId);
                    return new GeneralResult(false, _messages.MsgQuestionNotFound, null, ErrorType.NotFound);
                }

                // soft delete the question
                foreach (var choice in question.Choices)
                {
                    choice.IsDeleted = true;
                    choice.DeletedAt = now;
                }

                // soft delete the answers
                var answers = question.Test.Attempts
                    .SelectMany(a => a.Answers)
                    .Where(a => a.TestQuestionId == questionId)
                    .ToList();

                foreach (var answer in answers)
                {
                    answer.IsDeleted = true;
                    answer.DeletedAt = now;
                }

                // soft delete the question
                question.IsDeleted = true;
                question.DeletedAt = now;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("TestQuestionService - DeleteQuestionAsync : Question {QuestionId} deleted successfully.", questionId);
                return new GeneralResult(true, _messages.MsgQuestionDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "TestQuestionService - DeleteQuestionAsync : Error deleting question {QuestionId}.", questionId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Delete Question"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<QuestionDetailsDto>> GetQuestionByIdAsync(int questionId, CancellationToken cancellationToken)
        {
            try
            {
                var question = await _repository.GetByIdWithChoicesAndTestAsync(questionId, cancellationToken);
                if (question == null)
                {
                    _logger.LogWarning("TestQuestionService - GetQuestionByIdAsync : Question with ID {QuestionId} not found.", questionId);
                    return new GeneralResult<QuestionDetailsDto>(false, _messages.MsgQuestionNotFound, null, ErrorType.NotFound);
                }

                var dto = new QuestionDetailsDto
                {
                    TestId = question.TestId,
                    TestTitle = question.Test.Title,
                    LessonId = question.Test.LessonId,
                    LessonName = question.Test.CourseLesson?.Name ?? string.Empty,
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    QuestionMark = question.Mark,
                    Choices = question.Choices
                        .Where(c => !c.IsDeleted)
                        .Select(c => new RelatedQuestionChoiceDetailsDto
                        {
                            Id = c.Id,
                            Text = c.Text,
                            IsCorrect = c.IsCorrect
                        }).ToList()
                };

                _logger.LogInformation("TestQuestionService - GetQuestionByIdAsync : Retrieved question {QuestionId}.", questionId);
                return new GeneralResult<QuestionDetailsDto>(true, _messages.MsgQuestionRetrieved, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestQuestionService - GetQuestionByIdAsync : Error retrieving question {QuestionId}.", questionId);
                return new GeneralResult<QuestionDetailsDto>(false, _messages.GetUnexpectedErrorMessage("Get Question"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<PagedResult<QuestionDetailsDto>>> GetQuestionsByTestIdAsync(int testId, PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            var test = await _repository.GetTestWithQuestionsAsync(testId, cancellationToken);
            if (test == null) return new(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);

            var (items, totalCount) = await _repository.GetPagedQuestionsByTestIdAsync(testId, pagination.PageNumber, pagination.PageSize, cancellationToken);

            var dtos = items.Select(q => new QuestionDetailsDto
            {
                TestId = q.TestId,
                TestTitle = test.Title,
                LessonId = test.LessonId,
                LessonName = test.CourseLesson?.Name ?? string.Empty,
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                QuestionMark = q.Mark,
                Choices = q.Choices.Where(c => !c.IsDeleted).Select(c => new RelatedQuestionChoiceDetailsDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                }).ToList()
            }).ToList();

            return new(true, _messages.MsgQuestionsRetrieved, new PagedResult<QuestionDetailsDto>
            {
                Items = dtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            }, ErrorType.Success);
        }

        /// <inheritdoc />
        public async Task<GeneralResult> ReorderQuestionsAsync(int testId, List<ReorderDto> reorderList, CancellationToken cancellationToken)
        {
            var ids = reorderList.Select(x => x.Id).ToList();
            var questions = await _repository.GetQuestionsByIdsAsync(testId, ids, cancellationToken);

            if (questions.Count != reorderList.Count) return new(false, _messages.MsgReorderMismatch, null, ErrorType.BadRequest);

            foreach (var item in reorderList)
            {
                var q = questions.FirstOrDefault(x => x.Id == item.Id);
                if (q != null)
                {
                    q.DisplayOrder = item.DisplayOrder;
                    q.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new(true, _messages.MsgReorderSuccess, null, ErrorType.Success);
        }
    }
}
