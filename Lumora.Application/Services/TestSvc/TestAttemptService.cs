namespace Lumora.Application.Services.TestSvc
{
    public class TestAttemptService(
            ITestAttemptRepository attemptRepository,
            ITestRepository testRepository,
            ITestQuestionRepository questionRepository,
            IUnitOfWork unitOfWork,
            ILogger<TestAttemptService> logger,
            TestMessage messages) : ITestAttemptService
    {
        private readonly ITestAttemptRepository _attemptRepository = attemptRepository;
        private readonly ITestRepository _testRepository = testRepository;
        private readonly ITestQuestionRepository _questionRepository = questionRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<TestAttemptService> _logger = logger;
        private readonly TestMessage _messages = messages;

        public async Task<GeneralResult<TestAttemptStartDto>> StartAttemptAsync(string userId, int testId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                if (string.IsNullOrWhiteSpace(userId)) return new GeneralResult<TestAttemptStartDto>(false, _messages.MsgUserIdRequired, null, ErrorType.BadRequest);

                var test = await _testRepository.GetTestByIdAsync(testId, false, cancellationToken);
                if (test == null) return new GeneralResult<TestAttemptStartDto>(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);

                if (test.MaxAttempts != 0)
                {
                    var count = await _attemptRepository.GetUserAttemptsCountAsync(userId, testId, cancellationToken);
                    if (count >= test.MaxAttempts) return new GeneralResult<TestAttemptStartDto>(false, _messages.MsgMaxAttemptsReached, null, ErrorType.BadRequest);
                }

                var attempt = new TestAttempt { UserId = userId, TestId = test.Id, StartedAt = now, CreatedAt = now };
                await _attemptRepository.AddAttemptAsync(attempt, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult<TestAttemptStartDto>(true, _messages.MsgAttemptStarted, new TestAttemptStartDto
                {
                    AttemptId = attempt.Id,
                    TestId = test.Id,
                    TestTitle = test.Title,
                    StartedAt = attempt.StartedAt,
                    DurationInMinutes = test.DurationInMinutes
                }, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StartAttemptAsync Error");
                return new GeneralResult<TestAttemptStartDto>(false, _messages.GetUnexpectedErrorMessage("Start Attempt"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> SubmitAnswerAsync(TestAnswerDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null) return new GeneralResult(false, _messages.MsgAnswerDtoNull, null, ErrorType.BadRequest);

                var attempt = await _attemptRepository.GetByIdWithAnswersAsync(dto.AttemptId, cancellationToken);
                if (attempt == null) return new GeneralResult(false, _messages.MsgAttemptNotFound, null, ErrorType.NotFound);
                if (attempt.SubmittedAt != null) return new GeneralResult(false, _messages.MsgAttemptAlreadySubmitted, null, ErrorType.BadRequest);

                var test = await _testRepository.GetTestByIdAsync(attempt.TestId, false, cancellationToken);
                if (test == null) return new GeneralResult(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);

                if (DateTimeOffset.UtcNow > attempt.StartedAt.AddMinutes(test.DurationInMinutes))
                    return new GeneralResult(false, _messages.MsgAttemptExpired, null, ErrorType.BadRequest);

                var choice = await _questionRepository.GetChoiceByIdAsync(dto.SelectedChoiceId, cancellationToken);
                if (choice == null || choice.TestQuestionId != dto.QuestionId)
                    return new GeneralResult(false, _messages.MsgChoiceNotFound, null, ErrorType.BadRequest);

                var existing = attempt.Answers.FirstOrDefault(a => a.TestQuestionId == dto.QuestionId);
                if (existing != null) _attemptRepository.RemoveAnswer(existing);

                await _attemptRepository.AddAnswerAsync(new TestAnswer
                {
                    TestAttemptId = dto.AttemptId,
                    TestQuestionId = dto.QuestionId,
                    SelectedChoiceId = dto.SelectedChoiceId,
                    IsCorrect = choice.IsCorrect,
                    CreatedAt = DateTimeOffset.UtcNow
                }, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new GeneralResult(true, _messages.MsgAnswerSubmitted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubmitAnswerAsync Error");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Submit Answer"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> SubmitAttemptAsync(int attemptId, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTimeOffset.UtcNow;
                var attempt = await _attemptRepository.GetByIdWithAnswersAsync(attemptId, cancellationToken);
                if (attempt == null || attempt.SubmittedAt != null)
                    return new GeneralResult(false, _messages.MsgAttemptNotFound, null, ErrorType.NotFound);

                var test = await _testRepository.GetTestByIdAsync(attempt.TestId, false, cancellationToken);
                if (test == null) return new GeneralResult(false, _messages.MsgTestNotFound, null, ErrorType.NotFound);

                if (test.DurationInMinutes > 0 && now > attempt.StartedAt.AddMinutes(test.DurationInMinutes))
                    return new GeneralResult(false, _messages.MsgAttemptTimeExpired, null, ErrorType.BadRequest);

                var totalMark = await _attemptRepository.GetTotalTestMarkAsync(attempt.TestId, cancellationToken);
                var correctAnswers = await _attemptRepository.GetCorrectAnswersWithMarksAsync(attempt.Id, cancellationToken);
                var userMark = correctAnswers.Sum(a => a.TestQuestion.Mark);

                var bestMark = await _attemptRepository.GetUserBestScoreAsync(attempt.UserId, attempt.TestId, attempt.Id, cancellationToken);
                if (bestMark.HasValue && userMark < bestMark.Value)
                {
                    attempt.IsValidSubmission = false;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return new GeneralResult(false, _messages.MsgAttemptLowerThanPrevious, null, ErrorType.BadRequest);
                }

                attempt.SubmittedAt = now;
                attempt.TotalMark = userMark;
                attempt.IsPassed = userMark >= (totalMark * 0.5m);
                attempt.IsValidSubmission = true;
                attempt.UpdatedAt = now;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new GeneralResult(true, _messages.MsgAttemptSubmitted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "SubmitAttemptAsync Error");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Submit Attempt"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<TestAttemptResultDto>> GetBestAttemptResultAsync(string userId, int testId, CancellationToken cancellationToken)
        {
            var best = await _attemptRepository.GetBestAttemptWithDetailsAsync(userId, testId, cancellationToken);
            if (best == null) return new GeneralResult<TestAttemptResultDto>(false, _messages.MsgAttemptNotFound, null, ErrorType.NotFound);

            var totalQuestions = await _questionRepository.GetQuestionsCountAsync(testId, cancellationToken);

            var dto = new TestAttemptResultDto
            {
                AttemptId = best.Id,
                Score = best.TotalMark,
                IsPassed = best.IsPassed,
                StartedAt = best.StartedAt,
                SubmittedAt = best.SubmittedAt,
                TestTitle = best.Test.Title,
                TotalQuestions = totalQuestions,
                CorrectAnswers = best.Answers.Count(a => a.IsCorrect),
                Answers = best.Answers.Select(ans => new TestAnswerReviewDto
                {
                    QuestionId = ans.TestQuestionId,
                    QuestionText = ans.TestQuestion.QuestionText,
                    SelectedChoiceId = ans.SelectedChoiceId,
                    SelectedChoiceText = ans.TestChoice.Text,
                    IsCorrect = ans.IsCorrect
                }).ToList()
            };
            return new GeneralResult<TestAttemptResultDto>(true, _messages.MsgAttemptResultRetrieved, dto, ErrorType.Success);
        }

        public async Task<GeneralResult<PagedResult<TestAttemptSummaryDto>>> GetUserAttemptsAsync(string userId, int testId, PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            var paged = await _attemptRepository.GetUserAttemptsPagedAsync(userId, testId, pagination.Skip, pagination.PageSize, cancellationToken);

            var result = new PagedResult<TestAttemptSummaryDto>
            {
                Items = paged.Items.Select(a => new TestAttemptSummaryDto { AttemptId = a.Id, StartedAt = a.StartedAt, SubmittedAt = a.SubmittedAt, Score = a.TotalMark, IsPassed = a.IsPassed }).ToList(),
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = paged.TotalCount
            };
            return new GeneralResult<PagedResult<TestAttemptSummaryDto>>(true, _messages.MsgAttemptsRetrieved, result, ErrorType.Success);
        }
    }
}
