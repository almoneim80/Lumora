using Lumora.DTOs.Test;
using Lumora.Extensions;
using Lumora.Interfaces.TestIntf;
namespace Lumora.Services.TestSvc
{
    public class TestQuestionService(PgDbContext dbContext, ILogger<TestQuestionService> logger, TestMessage messages) : ITestQuestionService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<TestQuestionService> _logger = logger;
        private readonly TestMessage _messages = messages;

        /// <inheritdoc />
        public async Task<GeneralResult<int>> AddQuestionAsync(QuestionWithChoiseCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("TestQuestionService - AddQuestionAsync : TestQuestionCreateDto is null");
                    return new GeneralResult<int>(false, _messages.MsgQuestionDtoNull, 0, ErrorType.BadRequest);
                }

                if (dto.TestId <= 0)
                {
                    _logger.LogWarning("TestQuestionService - AddQuestionAsync : TestId is invalid");
                    return new GeneralResult<int>(false, _messages.MsgTestIdInvalid, 0, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(dto.Question))
                {
                    _logger.LogWarning("TestQuestionService - AddQuestionAsync : Question text is required");
                    return new GeneralResult<int>(false, _messages.MsgQuestionTextRequired, 0, ErrorType.BadRequest);
                }

                if (dto.Mark <= 0)
                {
                    _logger.LogWarning("TestQuestionService - AddQuestionAsync : Question mark must be greater than zero.");
                    return new GeneralResult<int>(false, _messages.MsgQuestionMarkMustBePositive, 0, ErrorType.BadRequest);
                }

                if (dto.Choices == null || dto.Choices.Count < 2)
                {
                    _logger.LogWarning("TestQuestionService - AddQuestionAsync : At least two choices are required");
                    return new GeneralResult<int>(false, _messages.MsgAtLeastTwoChoices, 0, ErrorType.BadRequest);
                }

                if (!dto.Choices.Exists(c => c.IsCorrect))
                {
                    _logger.LogWarning("TestQuestionService - AddQuestionAsync : At least one correct choice is required");
                    return new GeneralResult<int>(false, _messages.MsgAtLeastOneCorrectChoice, 0, ErrorType.BadRequest);
                }

                var correctChoicesCount = dto.Choices.Count(c => c.IsCorrect);
                if (correctChoicesCount != 1)
                {
                    _logger.LogWarning("TestQuestionService - AddQuestionAsync : Question must have exactly one correct choice.");
                    return new GeneralResult<int>(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, 0, ErrorType.BadRequest);
                }

                var testExists = await _dbContext.Tests.AnyAsync(t => t.Id == dto.TestId && !t.IsDeleted, cancellationToken);
                if (!testExists)
                {
                    _logger.LogWarning("TestQuestionService - AddQuestionAsync : Test with ID {TestId} not found", dto.TestId);
                    return new GeneralResult<int>(false, _messages.MsgTestNotFound, 0, ErrorType.NotFound);
                }

                var existingQuestionsCount = await _dbContext.TestQuestions.CountAsync(q => q.TestId == dto.TestId && !q.IsDeleted, cancellationToken);
                var question = new TestQuestion
                {
                    TestId = dto.TestId,
                    QuestionText = dto.Question.Trim(),
                    Mark = dto.Mark,
                    DisplayOrder = existingQuestionsCount + 1,
                    CreatedAt = now
                };

                await _dbContext.TestQuestions.AddAsync(question, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                for (int i = 0; i < dto.Choices.Count; i++)
                {
                    var c = dto.Choices[i];
                    var choice = new TestChoice
                    {
                        TestQuestionId = question.Id,
                        Text = c.Text.Trim(),
                        IsCorrect = c.IsCorrect,
                        DisplayOrder = i + 1,
                        CreatedAt = now
                    };

                    await _dbContext.TestChoices.AddAsync(choice, cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("TestQuestionService - AddQuestionAsync : Question added successfully to test {TestId}", dto.TestId);

                return new GeneralResult<int>(true, _messages.MsgQuestionCreated, question.Id, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestQuestionService - AddQuestionAsync : Error adding question to test {TestId}", dto?.TestId);
                return new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("Add Question"), 0, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> UpdateQuestionAsync(int questionId, TestQuestionUpdateDto dto, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("TestQuestionService - UpdateQuestionAsync : TestQuestionUpdateDto is null");
                    return new GeneralResult(false, _messages.MsgQuestionDtoNull, null, ErrorType.BadRequest);
                }

                var question = await _dbContext.TestQuestions
                    .Include(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.Id == questionId && !q.IsDeleted, cancellationToken);

                if (question == null)
                {
                    _logger.LogWarning("TestQuestionService - UpdateQuestionAsync : Question {QuestionId} not found", questionId);
                    return new GeneralResult(false, _messages.MsgQuestionNotFound, null, ErrorType.NotFound);
                }

                if (!string.IsNullOrWhiteSpace(dto.Question))
                    question.QuestionText = dto.Question.Trim();

                if (dto.Mark.HasValue)
                    question.Mark = dto.Mark.Value;

                if (dto.Mark.HasValue)
                {
                    if (dto.Mark.Value <= 0)
                    {
                        _logger.LogWarning("TestQuestionService - UpdateQuestionAsync : Question mark must be greater than zero.");
                        return new GeneralResult(false, _messages.MsgQuestionMarkMustBePositive, null, ErrorType.BadRequest);
                    }
                    question.Mark = dto.Mark.Value;
                }

                // update existing choicess
                foreach (var cDto in dto.Choices ?? [])
                {
                    if (cDto.Id.HasValue)
                    {
                        var existing = _dbContext.TestChoices.FirstOrDefault(c => c.Id == cDto.Id.Value);
                        if(existing == null)
                        {
                            _logger.LogWarning("TestQuestionService - UpdateQuestionAsync : Choice with ID {ChoiceId} not found.", cDto.Id.Value);
                            return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.NotFound);
                        }

                        if (existing != null)
                        {
                            if (!string.IsNullOrWhiteSpace(cDto.Text)) existing.Text = cDto.Text.Trim();
                            if (cDto.IsCorrect.HasValue) existing.IsCorrect = cDto.IsCorrect.Value;
                        }
                    }
                    else
                    {
                        var maxDisplayOrder = question.Choices.Where(c => !c.IsDeleted).Select(c => (int?)c.DisplayOrder).Max() ?? 0;
                        var newChoice = new TestChoice
                        {
                            TestQuestionId = questionId,
                            Text = cDto.Text?.Trim() ?? "",
                            IsCorrect = cDto.IsCorrect ?? false,
                            DisplayOrder = maxDisplayOrder + 1,
                            CreatedAt = now
                        };
                        await _dbContext.TestChoices.AddAsync(newChoice, cancellationToken);
                    }
                }

                var updatedChoices = question.Choices.Where(c => !c.IsDeleted).ToList();
                var correctChoicesCount = updatedChoices.Count(c => c.IsCorrect);
                if (correctChoicesCount != 1)
                {
                    _logger.LogWarning("TestQuestionService - UpdateQuestionAsync : Question {QuestionId} must have exactly one correct choice.", questionId);
                    return new GeneralResult(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, null, ErrorType.BadRequest);
                }

                question.UpdatedAt = now;
                var test = await _dbContext.Tests.Include(t => t.Questions)
                    .FirstOrDefaultAsync(t => t.Id == question.TestId && !t.IsDeleted, cancellationToken);

                if (test != null)
                {
                    test.TotalMark = test.Questions
                        .Where(q => !q.IsDeleted)
                        .Sum(q => q.Mark);
                    test.UpdatedAt = now;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("TestQuestionService - UpdateQuestionAsync : Question {QuestionId} updated successfully", questionId);
                return new GeneralResult(true, _messages.MsgQuestionUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "TestQuestionService - UpdateQuestionAsync : Error updating question {QuestionId}", questionId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Update Question"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> DeleteQuestionAsync(int questionId, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;

                var question = await _dbContext.TestQuestions
                    .Include(q => q.Choices)
                    .Include(q => q.Test)
                        .ThenInclude(t => t.Attempts)
                            .ThenInclude(a => a.Answers)
                    .FirstOrDefaultAsync(q => q.Id == questionId && !q.IsDeleted, cancellationToken);

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

                await _dbContext.SaveChangesAsync(cancellationToken);
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
                var question = await _dbContext.TestQuestions
                    .AsNoTracking()
                    .Include(q => q.Choices)
                    .Include(q => q.Test)
                        .ThenInclude(t => t.CourseLesson)
                    .FirstOrDefaultAsync(q => q.Id == questionId && !q.IsDeleted, cancellationToken);

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
            try
            {
                if (testId <= 0)
                {
                    _logger.LogWarning("TestQuestionService - GetQuestionsByTestIdAsync : Invalid test ID {TestId}.", testId);
                    return new GeneralResult<PagedResult<QuestionDetailsDto>>(false, _messages.MsgTestIdInvalid, null, ErrorType.BadRequest);
                }

                var testExists = await _dbContext.Tests
                    .AsNoTracking()
                    .Include(t => t.CourseLesson)
                    .FirstOrDefaultAsync(t => t.Id == testId && !t.IsDeleted, cancellationToken);

                if (testExists == null)
                {
                    _logger.LogWarning("TestQuestionService - GetQuestionsByTestIdAsync : Test with ID {TestId} not found.", testId);
                    return new GeneralResult<PagedResult<QuestionDetailsDto>>(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);
                }

                var query = _dbContext.TestQuestions
                    .AsNoTracking()
                    .Where(q => q.TestId == testId && !q.IsDeleted)
                    .Include(q => q.Choices)
                    .OrderByDescending(q => q.CreatedAt);

                var pagedEntities = await query.ApplyPaginationAsync(pagination, cancellationToken);

                if (!pagedEntities.Items.Any())
                {
                    _logger.LogInformation("TestQuestionService - GetQuestionsByTestIdAsync : No questions found for test {TestId}.", testId);
                    return new GeneralResult<PagedResult<QuestionDetailsDto>>(false, _messages.MsgNoQuestionsFound, null, ErrorType.NotFound);
                }

                var dtos = pagedEntities.Items.Select(question => new QuestionDetailsDto
                {
                    TestId = question.TestId,
                    TestTitle = testExists.Title,
                    LessonId = testExists.LessonId,
                    LessonName = testExists.CourseLesson?.Name ?? string.Empty,
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
                }).ToList();

                var result = new PagedResult<QuestionDetailsDto>
                {
                    Items = dtos,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalCount = pagedEntities.TotalCount
                };

                _logger.LogInformation($"TestQuestionService - GetQuestionsByTestIdAsync : Retrieved {result.TotalCount} questions for test {testId}.");
                return new GeneralResult<PagedResult<QuestionDetailsDto>>(true, _messages.MsgQuestionsRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestQuestionService - GetQuestionsByTestIdAsync : Error retrieving questions for test {TestId}.", testId);
                return new GeneralResult<PagedResult<QuestionDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get Questions"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> ReorderQuestionsAsync(int testId, List<ReorderDto> reorderList, CancellationToken cancellationToken)
        {
            try
            {
                if (testId <= 0)
                {
                    _logger.LogWarning("TestQuestionService - ReorderQuestionsAsync : Invalid test ID.");
                    return new GeneralResult(false, _messages.MsgTestIdInvalid, null, ErrorType.BadRequest);
                }

                if (reorderList == null || !reorderList.Any())
                {
                    _logger.LogWarning("TestQuestionService - ReorderQuestionsAsync : Reorder list is null or empty.");
                    return new GeneralResult(false, _messages.MsgReorderListEmpty, null, ErrorType.BadRequest);
                }

                var questionIds = reorderList.Select(x => x.Id).ToList();
                var distinctOrders = reorderList.Select(x => x.DisplayOrder).Distinct().ToList();

                if (distinctOrders.Count != reorderList.Count)
                {
                    _logger.LogWarning("TestQuestionService - ReorderQuestionsAsync : Duplicate display orders found.");
                    return new GeneralResult(false, _messages.MsgReorderHasDuplicates, null, ErrorType.BadRequest);
                }

                var questions = await _dbContext.TestQuestions
                    .Where(q => q.TestId == testId && !q.IsDeleted && questionIds.Contains(q.Id))
                    .ToListAsync(cancellationToken);

                if (questions.Count != reorderList.Count)
                {
                    _logger.LogWarning("TestQuestionService - ReorderQuestionsAsync : Mismatch in reorder items and actual questions.");
                    return new GeneralResult(false, _messages.MsgReorderMismatch, null, ErrorType.BadRequest);
                }

                foreach (var item in reorderList)
                {
                    var question = questions.FirstOrDefault(q => q.Id == item.Id);
                    if (question != null)
                    {
                        question.DisplayOrder = item.DisplayOrder;
                        question.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("TestQuestionService - ReorderQuestionsAsync : Successfully updated display order for test {TestId}", testId);
                return new GeneralResult(true, _messages.MsgReorderSuccess, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestQuestionService - ReorderQuestionsAsync : Error updating question order for test {TestId}", testId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Reorder Questions"), null, ErrorType.InternalServerError);
            }
        }
    }
}
