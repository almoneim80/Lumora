using Lumora.DTOs.Test;
using Lumora.Extensions;
using Lumora.Interfaces.TestIntf;
namespace Lumora.Services.TestSvc
{
    public class TestService(PgDbContext dbContext, ILogger<TestService> logger, TestMessage messages) : ITestService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<TestService> _logger = logger;
        private readonly TestMessage _messages = messages;

        /// <inheritdoc />
        public async Task<GeneralResult<int>> CreateTestAsync(TestCreateDto dto, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;
                if (dto == null)
                {
                    _logger.LogWarning("TestService - CreateTestAsync : TestCreateDto is null");
                    return new GeneralResult<int>(false, _messages.MsgTestDtoNull, 0, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return new GeneralResult<int>(false, _messages.MsgTestTitleRequired, 0, ErrorType.BadRequest);

                if (dto.LessonId <= 0)
                {
                    _logger.LogWarning("TestService - CreateTestAsync : LessonId is invalid");
                    return new GeneralResult<int>(false, _messages.MsgLessonIdInvalid, 0, ErrorType.BadRequest);
                }

                if (dto.Questions == null || !dto.Questions.Any())
                {
                    _logger.LogWarning("TestService - CreateTestAsync : Test must have at least one question");
                    return new GeneralResult<int>(false, _messages.MsgTestMustHaveQuestions, 0, ErrorType.BadRequest);
                }

                var existingActiveTest = await _dbContext.Tests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.LessonId == dto.LessonId && !t.IsDeleted, cancellationToken);

                if (existingActiveTest != null)
                {
                    _logger.LogWarning("TestService - CreateTestAsync : An active test already exists for lesson {LessonId}.", dto.LessonId);
                    return new GeneralResult<int>(false, _messages.MsgTestAlreadyExistsForLesson, 0, ErrorType.Conflict);
                }


                foreach (var question in dto.Questions)
                {
                    if (string.IsNullOrWhiteSpace(question.Text))
                    {
                        _logger.LogWarning("TestService - CreateTestAsync : Test question text is required");
                        return new GeneralResult<int>(false, _messages.MsgTestQuestionTextRequired, 0, ErrorType.BadRequest);
                    }

                    if (question.Choices == null || question.Choices.Count < 2)
                    {
                        _logger.LogWarning("TestService - CreateTestAsync : Test question must have at least two choices");
                        return new GeneralResult<int>(false, _messages.MsgTestAtLeastTwoChoices, 0, ErrorType.BadRequest);
                    }

                    if (!HasExactlyOneCorrectChoice(question.Choices))
                    {
                        _logger.LogWarning("TestService - CreateTestAsync : Question must have exactly one correct choice");
                        return new GeneralResult<int>(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, 0, ErrorType.BadRequest);
                    }
                }

                var lesson = await _dbContext.CourseLessons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Id == dto.LessonId && !l.IsDeleted, cancellationToken);

                if (lesson == null)
                {
                    _logger.LogWarning("TestService - CreateTestAsync : Lesson with ID {LessonId} not found or deleted", dto.LessonId);
                    return new GeneralResult<int>(false, _messages.MsgLessonNotFound, 0, ErrorType.NotFound);
                }

                var test = new Test
                {
                    LessonId = dto.LessonId,
                    Title = dto.Title.Trim(),
                    DurationInMinutes = dto.DurationInMinutes,
                    TotalMark = dto.Questions.Sum(q => q.Mark),
                    CreatedAt = now,
                };

                await _dbContext.Tests.AddAsync(test, cancellationToken);

                int questionOrder = 0;
                foreach (var qDto in dto.Questions)
                {
                    var question = new TestQuestion
                    {
                        Test = test,
                        QuestionText = qDto.Text.Trim(),
                        Mark = qDto.Mark,
                        CreatedAt = now,
                        DisplayOrder = questionOrder++
                    };
                    await _dbContext.TestQuestions.AddAsync(question, cancellationToken);

                    int choiceOrder = 0;
                    foreach (var cDto in qDto.Choices)
                    {
                        var choice = new TestChoice
                        {
                            TestQuestion = question,
                            Text = cDto.Text.Trim(),
                            IsCorrect = cDto.IsCorrect,
                            CreatedAt = now,
                            DisplayOrder = choiceOrder++
                        };
                        await _dbContext.TestChoices.AddAsync(choice, cancellationToken);
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("TestService - CreateTestAsync : Test created successfully for lesson {LessonId}", dto.LessonId);
                return new GeneralResult<int>(true, _messages.MsgTestCreated, test.Id, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestService - CreateTestAsync : Error creating test for lesson {LessonId}", dto?.LessonId);
                await transaction.RollbackAsync(cancellationToken);
                return new GeneralResult<int>(false, _messages.GetUnexpectedErrorMessage("Create Test"), 0, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> UpdateTestAsync(int testId, TestUpdateDto dto, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;
                var totalMark = 0m;

                var test = await _dbContext.Tests
                    .Include(t => t.Questions)
                        .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(t => t.Id == testId && !t.IsDeleted, cancellationToken);

                if (test == null)
                {
                    _logger.LogWarning("TestService - UpdateTestAsync : Test with ID {TestId} not found.", testId);
                    return new GeneralResult(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);
                }

                if (!string.IsNullOrWhiteSpace(dto.Title)) test.Title = dto.Title.Trim();
                if (dto.DurationInMinutes.HasValue) test.DurationInMinutes = dto.DurationInMinutes.Value;

                foreach (var qDto in dto.Questions ?? [])
                {
                    TestQuestion? question;

                    if (qDto.Id.HasValue)
                    {
                        // Existing question
                        question = test.Questions.FirstOrDefault(q => q.Id == qDto.Id.Value);
                        if (question is null)
                        {
                            _logger.LogWarning("TestService - UpdateTestAsync : Question ID {QuestionId} not found for update.", qDto.Id.Value);
                            return new GeneralResult(false, _messages.MsgQuestionNotFound, null, ErrorType.NotFound);
                        }

                        if (!string.IsNullOrWhiteSpace(qDto.Question)) question.QuestionText = qDto.Question.Trim();
                        if (qDto.Mark.HasValue) question.Mark = qDto.Mark.Value;
                        if (question.Mark <= 0)
                        {
                            _logger.LogWarning("TestService - UpdateTestAsync : Question {QuestionId} has non-positive mark.", question.Id);
                            return new GeneralResult(false, _messages.MsgTestQuestionMarkRequired, null, ErrorType.BadRequest);
                        }
                    }
                    else
                    {
                        // New question
                        question = new TestQuestion
                        {
                            TestId = test.Id,
                            QuestionText = qDto.Question?.Trim() ?? "",
                            Mark = qDto.Mark.GetValueOrDefault(1),
                            CreatedAt = now
                        };

                        var maxOrder = test.Questions.Any() ? test.Questions.Max(q => q.DisplayOrder) : 0;
                        question.DisplayOrder = maxOrder + 1;

                        await _dbContext.TestQuestions.AddAsync(question, cancellationToken);
                        test.Questions.Add(question);
                    }

                    totalMark += question.Mark;

                    foreach (var cDto in qDto.Choices ?? [])
                    {
                        if (cDto.Id.HasValue)
                        {
                            // Update existing
                            var choice = question.Choices.FirstOrDefault(c => c.Id == cDto.Id.Value);
                            if (choice == null) continue;

                            if (!string.IsNullOrWhiteSpace(cDto.Text)) choice.Text = cDto.Text.Trim();
                            if (cDto.IsCorrect.HasValue) choice.IsCorrect = cDto.IsCorrect.Value;
                        }
                        else
                        {
                            var maxChoiceOrder = question.Choices.Any() ? question.Choices.Max(c => c.DisplayOrder) : 0;

                            // New choice
                            var choice = new TestChoice
                            {
                                TestQuestionId = question.Id,
                                Text = cDto.Text?.Trim() ?? "",
                                IsCorrect = cDto.IsCorrect ?? false,
                                DisplayOrder = maxChoiceOrder + 1,
                                CreatedAt = now
                            };

                            await _dbContext.TestChoices.AddAsync(choice, cancellationToken);
                            question.Choices.Add(choice);
                        }
                    }

                    // enforce only one IsCorrect = true
                    var correctChoices = question.Choices.Where(c => !c.IsDeleted && c.IsCorrect).ToList();
                    if (correctChoices.Count != 1)
                    {
                        _logger.LogWarning("TestService - UpdateTestAsync : Question {QuestionId} must have exactly one correct choice.", question.Id);
                        return new GeneralResult(false, _messages.MsgTestMustHaveOneCorrectChoiceOnly, null, ErrorType.BadRequest);
                    }
                }

                test.TotalMark = decimal.Round(totalMark, 2, MidpointRounding.AwayFromZero);
                test.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("TestService - UpdateTestAsync : Test {TestId} updated successfully.", testId);
                return new GeneralResult(true, _messages.MsgTestUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestService - UpdateTestAsync : Error updating test {TestId}.", testId);
                await transaction.RollbackAsync(cancellationToken);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Update Test"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> DeleteTestAsync(int testId, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;

                var test = await _dbContext.Tests
                    .Include(t => t.Questions)
                        .ThenInclude(q => q.Choices)
                    .Include(t => t.Attempts)
                        .ThenInclude(a => a.Answers)
                    .FirstOrDefaultAsync(t => t.Id == testId && !t.IsDeleted, cancellationToken);

                if (test == null)
                {
                    _logger.LogWarning("TestService - DeleteTestAsync : Test with ID {TestId} not found.", testId);
                    return new GeneralResult(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);
                }

                foreach (var question in test.Questions)
                {
                    question.IsDeleted = true;
                    question.DeletedAt = now;

                    foreach (var choice in question.Choices)
                    {
                        choice.IsDeleted = true;
                        choice.DeletedAt = now;
                    }
                }

                foreach (var attempt in test.Attempts)
                {
                    attempt.IsDeleted = true;
                    attempt.DeletedAt = now;

                    foreach (var answer in attempt.Answers)
                    {
                        answer.IsDeleted = true;
                        answer.DeletedAt = now;
                    }
                }

                // soft delete of test
                test.IsDeleted = true;
                test.DeletedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("TestService - DeleteTestAsync : Test {TestId} deleted successfully.", testId);
                return new GeneralResult(true, _messages.MsgTestDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "TestService - DeleteTestAsync : Error deleting test with ID {TestId}.", testId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Delete Test"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<TestDetailsDto>> GetTestByIdAsync(int testId, CancellationToken cancellationToken)
        {
            try
            {
                var test = await _dbContext.Tests
                    .AsNoTracking()
                    .Include(t => t.Questions)
                        .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(t => t.Id == testId && !t.IsDeleted, cancellationToken);

                if (test == null)
                {
                    _logger.LogWarning("TestService - GetTestByIdAsync : Test with ID {TestId} not found.", testId);
                    return new GeneralResult<TestDetailsDto>(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);
                }

                var dto = new TestDetailsDto
                {
                    Id = test.Id,
                    LessonId = test.LessonId,
                    LessonName = test.CourseLesson?.Name ?? _messages.MsgLessonNameUnavailable,
                    Title = test.Title,
                    DurationInMinutes = test.DurationInMinutes,
                    TotalMark = test.TotalMark,
                    Questions = test.Questions
                        .Where(q => !q.IsDeleted)
                        .Select(q => new RelatedTestQuestionDetailsDto
                        {
                            Id = q.Id,
                            Question = q.QuestionText,
                            Mark = q.Mark,
                            Choices = q.Choices
                                .Where(c => !c.IsDeleted)
                                .Select(c => new RelatedTestChoiceDetailsDto
                                {
                                    Id = c.Id,
                                    Text = c.Text,
                                    IsCorrect = c.IsCorrect
                                }).ToList()
                        }).ToList()
                };

                _logger.LogInformation("TestService - GetTestByIdAsync : Test {TestId} retrieved successfully.", testId);
                return new GeneralResult<TestDetailsDto>(true, _messages.MsgTestRetrieved, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestService - GetTestByIdAsync : Error retrieving test {TestId}.", testId);
                return new GeneralResult<TestDetailsDto>(false, _messages.GetUnexpectedErrorMessage("Get Test"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<PagedResult<TestDetailsDto>>> GetAllTestsAsync(PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var query = _dbContext.Tests
                    .AsNoTracking()
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.CreatedAt)
                    .Include(t => t.CourseLesson)
                    .Include(t => t.Questions).ThenInclude(q => q.Choices);

                var pagedEntities = await query.ApplyPaginationAsync(pagination, cancellationToken);
                if (!pagedEntities.Items.Any())
                {
                    _logger.LogInformation("TestService - GetTestsForLessonAsync : No tests found.");
                    return new GeneralResult<PagedResult<TestDetailsDto>>(false, _messages.MsgTestsNotFound, null, ErrorType.NotFound);
                }

                var dtos = pagedEntities.Items.Select(test => new TestDetailsDto
                {
                    Id = test.Id,
                    LessonId = test.LessonId,
                    LessonName = test.CourseLesson.Name,
                    Title = test.Title,
                    DurationInMinutes = test.DurationInMinutes,
                    TotalMark = test.TotalMark,
                    Questions = test.Questions
                        .Where(q => !q.IsDeleted)
                        .Select(q => new RelatedTestQuestionDetailsDto
                        {
                            Id = q.Id,
                            Question = q.QuestionText,
                            Mark = q.Mark,
                            Choices = q.Choices
                                .Where(c => !c.IsDeleted)
                                .Select(c => new RelatedTestChoiceDetailsDto
                                {
                                    Id = c.Id,
                                    Text = c.Text,
                                    IsCorrect = c.IsCorrect
                                }).ToList()
                        }).ToList()
                }).ToList();

                var result = new PagedResult<TestDetailsDto>
                {
                    Items = dtos,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalCount = pagedEntities.TotalCount
                };

                _logger.LogInformation("TestService - GetTestsForLessonAsync : Retrieved {Count} tests.", dtos.Count);
                return new GeneralResult<PagedResult<TestDetailsDto>>(true, _messages.MsgTestsRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestService - GetTestsForLessonAsync : Error retrieving tests.");
                return new GeneralResult<PagedResult<TestDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Get Tests For Lesson"), null, ErrorType.InternalServerError);
            }
        }

        #region Private method
        private bool HasExactlyOneCorrectChoice(List<RelatedTestChoiceDto> choices)
        {
            return choices.Count(c => c.IsCorrect) == 1;
        }
        #endregion
    }
}
