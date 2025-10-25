using Lumora.DTOs.Test;
using Lumora.Extensions;
using Lumora.Interfaces.TestIntf;

namespace Lumora.Services.TestSvc
{
    public class TestAttemptService(PgDbContext dbContext, ILogger<TestAttemptService> logger, TestMessage messages) : ITestAttemptService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly ILogger<TestAttemptService> _logger = logger;
        private readonly TestMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult<TestAttemptStartDto>> StartAttemptAsync(string userId, int testId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("TestAttemptService - StartAttemptAsync : UserId is null or empty.");
                    return new GeneralResult<TestAttemptStartDto>(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                var test = await _dbContext.Tests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == testId && !t.IsDeleted, cancellationToken);

                if (test == null)
                {
                    _logger.LogWarning("TestAttemptService - StartAttemptAsync : Test with ID {TestId} not found.", testId);
                    return new GeneralResult<TestAttemptStartDto>(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);
                }

                if (test.MaxAttempts != 0)
                {
                    var previousAttemptsCount = await _dbContext.TestAttempts
                        .Where(a =>
                            a.TestId == testId &&
                            a.UserId == userId &&
                            !a.IsDeleted)
                        .CountAsync(cancellationToken);

                    if (previousAttemptsCount >= test.MaxAttempts)
                    {
                        _logger.LogWarning("TestAttemptService - StartAttemptAsync : Max attempts reached for Test {TestId}, User {UserId}", testId, userId);
                        return new GeneralResult<TestAttemptStartDto>(false, _messages.MsgMaxAttemptsReached, null, ErrorType.BadRequest);
                    }
                }

                var attempt = new TestAttempt
                {
                    UserId = userId,
                    TestId = test.Id,
                    StartedAt = now,
                    IsPassed = false,
                    TotalMark = 0,
                    CreatedAt = now
                };

                await _dbContext.TestAttempts.AddAsync(attempt, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var dto = new TestAttemptStartDto
                {
                    AttemptId = attempt.Id,
                    TestId = test.Id,
                    TestTitle = test.Title,
                    StartedAt = attempt.StartedAt,
                    DurationInMinutes = test.DurationInMinutes
                };

                _logger.LogInformation("TestAttemptService - StartAttemptAsync : Attempt {AttemptId} started for user {UserId}.", attempt.Id, userId);
                return new GeneralResult<TestAttemptStartDto>(true, _messages.MsgAttemptStarted, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestAttemptService - StartAttemptAsync : Error starting attempt for test {TestId}.", testId);
                return new GeneralResult<TestAttemptStartDto>(false, _messages.GetUnexpectedErrorMessage("Start Attempt"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SubmitAnswerAsync(TestAnswerDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (dto == null)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : TestAnswerDto is null");
                    return new GeneralResult(false, _messages.MsgAnswerDtoNull, null, ErrorType.BadRequest);
                }

                if (dto.AttemptId <= 0 || dto.QuestionId <= 0 || dto.SelectedChoiceId <= 0)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : Invalid data.");
                    return new GeneralResult(false, _messages.MsgAnswerInvalidData, null, ErrorType.BadRequest);
                }

                var attempt = await _dbContext.TestAttempts
                    .Include(a => a.Answers)
                    .FirstOrDefaultAsync(a => a.Id == dto.AttemptId && !a.IsDeleted, cancellationToken);

                if (attempt == null)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : Attempt with ID {AttemptId} not found", dto.AttemptId);
                    return new GeneralResult(false, _messages.MsgAttemptNotFound, null, ErrorType.NotFound);
                }

                if (attempt.SubmittedAt != null)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : Attempt already submitted.");
                    return new GeneralResult(false, _messages.MsgAttemptAlreadySubmitted, null, ErrorType.BadRequest);
                }

                var durationInMinutes = await _dbContext.Tests
                        .Where(t => t.Id == attempt.TestId && !t.IsDeleted)
                        .Select(t => (int?)t.DurationInMinutes)
                        .FirstOrDefaultAsync(cancellationToken);

                if (!durationInMinutes.HasValue)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : Test {TestId} not found.", attempt.TestId);
                    return new GeneralResult(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);
                }

                var testDeadline = attempt.StartedAt.AddMinutes(durationInMinutes.Value);
                if (now > testDeadline)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : Attempt {AttemptId} has expired (Deadline: {Deadline}).", attempt.Id, testDeadline);
                    return new GeneralResult(false, _messages.MsgAttemptExpired, null, ErrorType.BadRequest);
                }

                var questionExists = await _dbContext.TestQuestions.AnyAsync(q => q.Id == dto.QuestionId && !q.IsDeleted, cancellationToken);
                if (!questionExists)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : Question not found.");
                    return new GeneralResult(false, _messages.MsgQuestionNotFound, null, ErrorType.NotFound);
                }

                var choice = await _dbContext.TestChoices
                    .FirstOrDefaultAsync(c => c.Id == dto.SelectedChoiceId && !c.IsDeleted, cancellationToken);

                if (choice == null)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : Choice not found.");
                    return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.NotFound);
                }

                if (choice.TestQuestionId != dto.QuestionId)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAnswerAsync : Choice {ChoiceId} does not belong to question {QuestionId}.", dto.SelectedChoiceId, dto.QuestionId);
                    return new GeneralResult(false, _messages.MsgChoiceDoesNotBelongToQuestion, null, ErrorType.BadRequest);
                }

                // if answer already exists, remove it first then add the new one
                var existingAnswer = attempt.Answers.FirstOrDefault(a => a.TestQuestionId == dto.QuestionId);
                if (existingAnswer != null)
                {
                    _dbContext.TestAnswers.Remove(existingAnswer);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                var isCorrect = choice.IsCorrect;
                var answer = new TestAnswer
                {
                    TestAttemptId = dto.AttemptId,
                    TestQuestionId = dto.QuestionId,
                    SelectedChoiceId = dto.SelectedChoiceId,
                    IsCorrect = isCorrect,
                    CreatedAt = now
                };

                await _dbContext.TestAnswers.AddAsync(answer, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("TestAttemptService - SubmitAnswerAsync : Answer submitted for attempt {AttemptId}, question {QuestionId}", dto.AttemptId, dto.QuestionId);
                return new GeneralResult(true, _messages.MsgAnswerSubmitted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestAttemptService - SubmitAnswerAsync : Error submitting answer.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Submit Answer"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SubmitAttemptAsync(int attemptId, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;

                var attempt = await _dbContext.TestAttempts
                    .Include(a => a.Test)
                    .Include(a => a.Answers).ThenInclude(ans => ans.TestQuestion)
                    .FirstOrDefaultAsync(a => a.Id == attemptId && !a.IsDeleted, cancellationToken);

                if (attempt == null)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAttemptAsync : Attempt with ID {AttemptId} not found.", attemptId);
                    return new GeneralResult(false, _messages.MsgAttemptNotFound, null, ErrorType.NotFound);
                }

                if (attempt.SubmittedAt != null)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAttemptAsync : Attempt already submitted.");
                    return new GeneralResult(false, _messages.MsgAttemptAlreadySubmitted, null, ErrorType.BadRequest);
                }

                var testDuration = await _dbContext.Tests
                    .Where(t => t.Id == attempt.TestId && !t.IsDeleted)
                    .Select(t => (int?)t.DurationInMinutes)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!testDuration.HasValue)
                {
                    _logger.LogWarning("TestAttemptService - SubmitAttemptAsync : Test {TestId} not found.", attempt.TestId);
                    return new GeneralResult(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);
                }

                if (testDuration > 0 &&
                    DateTimeOffset.UtcNow > attempt.StartedAt.ToUniversalTime().AddMinutes(testDuration.Value))
                {
                    _logger.LogWarning("TestAttemptService - SubmitAttemptAsync : Attempt {AttemptId} exceeded allowed time.", attemptId);
                    return new GeneralResult(false, _messages.MsgAttemptTimeExpired, null, ErrorType.BadRequest);
                }

                var totalMark = await _dbContext.TestQuestions
                    .Where(q => q.TestId == attempt.TestId && !q.IsDeleted)
                    .SumAsync(q => q.Mark, cancellationToken);

                var correctAnswers = await _dbContext.TestAnswers
                    .Include(a => a.TestQuestion)
                    .Where(a => a.TestAttemptId == attempt.Id && !a.IsDeleted && a.IsCorrect)
                    .ToListAsync(cancellationToken);

                var userMark = correctAnswers.Sum(a => a.TestQuestion.Mark);

                var previousBestMark = await _dbContext.TestAttempts
                    .Where(a =>
                        a.UserId == attempt.UserId &&
                        a.TestId == attempt.TestId &&
                        a.SubmittedAt != null &&
                        !a.IsDeleted &&
                        a.Id != attempt.Id)
                    .MaxAsync(a => (decimal?)a.TotalMark, cancellationToken);

                if (previousBestMark.HasValue && userMark < previousBestMark.Value)
                {
                    attempt.IsValidSubmission = false;
                    attempt.UpdatedAt = now;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogWarning("TestAttemptService - SubmitAttemptAsync : Attempt {AttemptId} has lower mark than previous best. Marked as invalid.", attemptId);
                    return new GeneralResult(false, _messages.MsgAttemptLowerThanPrevious, null, ErrorType.BadRequest);
                }

                attempt.SubmittedAt = now;
                attempt.TotalMark = userMark;
                attempt.IsPassed = userMark >= (totalMark * 0.5m);
                attempt.UpdatedAt = now;
                attempt.IsValidSubmission = true;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("TestAttemptService - SubmitAttemptAsync : Attempt {AttemptId} submitted successfully.", attemptId);
                return new GeneralResult(true, _messages.MsgAttemptSubmitted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "TestAttemptService - SubmitAttemptAsync : Error submitting attempt {AttemptId}", attemptId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Submit Attempt"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<TestAttemptResultDto>> GetBestAttemptResultAsync(string userId, int testId, CancellationToken cancellationToken)
        {
            try
            {
                var bestAttempt = await _dbContext.TestAttempts
                    .AsNoTracking()
                    .Where(a =>
                        a.UserId.Equals(userId) &&
                        a.TestId == testId &&
                        !a.IsDeleted &&
                        a.SubmittedAt != null && a.IsValidSubmission)
                    .OrderByDescending(a => a.TotalMark)
                    .Select(a => new TestAttemptResultDto
                    {
                        AttemptId = a.Id,
                        UserId = a.UserId,
                        TestTitle = a.Test.Title,
                        TotalQuestions = _dbContext.TestQuestions.Count(q => q.TestId == a.TestId && !q.IsDeleted),
                        CorrectAnswers = a.Answers.Count(ans => ans.IsCorrect && !ans.IsDeleted),
                        Score = a.TotalMark,
                        IsPassed = a.IsPassed,
                        StartedAt = a.StartedAt,
                        SubmittedAt = a.SubmittedAt,
                        Answers = a.Answers
                            .Where(ans => !ans.IsDeleted)
                            .Select(ans => new TestAnswerReviewDto
                            {
                                QuestionId = ans.TestQuestionId,
                                QuestionText = ans.TestQuestion.QuestionText,
                                SelectedChoiceId = ans.SelectedChoiceId,
                                SelectedChoiceText = ans.TestChoice.Text,
                                IsCorrect = ans.IsCorrect
                            }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (bestAttempt == null)
                {
                    _logger.LogWarning("TestAttemptService - GetBestAttemptResultAsync : No attempts found for user {UserId} and test {TestId}.", userId, testId);
                    return new GeneralResult<TestAttemptResultDto>(false, _messages.MsgAttemptNotFound, null, ErrorType.NotFound);
                }

                _logger.LogInformation("TestAttemptService - GetBestAttemptResultAsync : Best attempt retrieved for user {UserId} and test {TestId}.", userId, testId);
                return new GeneralResult<TestAttemptResultDto>(true, _messages.MsgAttemptResultRetrieved, bestAttempt, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestAttemptService - GetBestAttemptResultAsync : Error retrieving best attempt for user {UserId} and test {TestId}.", userId, testId);
                return new GeneralResult<TestAttemptResultDto>(false, _messages.GetUnexpectedErrorMessage("Get Best Attempt Result"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<PagedResult<TestAttemptSummaryDto>>> GetUserAttemptsAsync(string userId, int testId, PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("TestAttemptService - GetUserAttemptsAsync : UserId is null or empty.");
                    return new GeneralResult<PagedResult<TestAttemptSummaryDto>>(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);
                }

                if (testId <= 0)
                {
                    _logger.LogWarning("TestAttemptService - GetUserAttemptsAsync : Invalid test ID {TestId}.", testId);
                    return new GeneralResult<PagedResult<TestAttemptSummaryDto>>(false, _messages.MsgTestIdInvalid, null, ErrorType.BadRequest);
                }

                var query = _dbContext.TestAttempts
                    .AsNoTracking()
                    .Where(a => a.UserId == userId && a.TestId == testId && !a.IsDeleted)
                    .OrderByDescending(a => a.StartedAt);

                var pagedAttempts = await query.ApplyPaginationAsync(pagination, cancellationToken);

                if (!pagedAttempts.Items.Any())
                {
                    _logger.LogInformation("TestAttemptService - GetUserAttemptsAsync : No attempts found for user {UserId} and test {TestId}.", userId, testId);
                    return new GeneralResult<PagedResult<TestAttemptSummaryDto>>(false, _messages.MsgNoAttemptsFound, null, ErrorType.NotFound);
                }

                var dtos = pagedAttempts.Items.Select(a => new TestAttemptSummaryDto
                {
                    AttemptId = a.Id,
                    StartedAt = a.StartedAt,
                    SubmittedAt = a.SubmittedAt,
                    Score = a.TotalMark,
                    IsPassed = a.IsPassed
                }).ToList();

                var result = new PagedResult<TestAttemptSummaryDto>
                {
                    Items = dtos,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalCount = pagedAttempts.TotalCount
                };

                _logger.LogInformation("TestAttemptService - GetUserAttemptsAsync : Retrieved {Count} attempts for user {UserId} and test {TestId}.", dtos.Count, userId, testId);
                return new GeneralResult<PagedResult<TestAttemptSummaryDto>>(true, _messages.MsgAttemptsRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestAttemptService - GetUserAttemptsAsync : Error retrieving attempts for user {UserId} and test {TestId}.", userId, testId);
                return new GeneralResult<PagedResult<TestAttemptSummaryDto>>(false, _messages.GetUnexpectedErrorMessage("Get User Attempts"), null, ErrorType.InternalServerError);
            }
        }
    }
}
