namespace Lumora.Application.Services.Programs
{
    public class CourseLessonService(
            ICourseLessonRepository repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            TestMessage testMessage,
            ILogger<CourseLessonService> logger,
            CourseLessonMessages messages,
            IQueryService queryService) : ICourseLessonService
    {
        private readonly ICourseLessonRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CourseLessonService> _logger = logger;
        private readonly CourseLessonMessages _messages = messages;
        private readonly IQueryService _queryService = queryService;

        public async Task<GeneralResult> CreateLessonWithContentAsync(int courseId, LessonsWithContentCreateDto dto, CancellationToken cancellationToken)
        {
            if (dto == null)
                return new GeneralResult(false, _messages.MsgNullOrEmpty, null, ErrorType.BadRequest);

            if (dto.Attachments?.Any() != true)
                return new GeneralResult(false, _messages.MsgLessonMustHaveAttachment, null, ErrorType.BadRequest);

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var courseExists = await _repository.CourseExistsAsync(courseId, cancellationToken);
                if (!courseExists)
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                var lesson = _mapper.Map<CourseLesson>(dto);
                lesson.ProgramCourseId = courseId;
                await _repository.AddLessonAsync(lesson, cancellationToken);

                var attachments = dto.Attachments.Select(attach =>
                {
                    var entity = _mapper.Map<LessonAttachment>(attach);
                    entity.CourseLesson = lesson;
                    return entity;
                }).ToList();

                await _repository.AddAttachmentsRangeAsync(attachments, cancellationToken);

                if (dto.Test is not null)
                {
                    if (dto.Test.Questions is null || dto.Test.Questions.Count == 0)
                        return new GeneralResult(false, testMessage.MsgTestMustHaveQuestions, null, ErrorType.BadRequest);

                    if (dto.Test.Questions.Exists(q => q.Choices.Count < 2))
                        return new GeneralResult(false, _messages.MsgAtLeastTwoChoices, null, ErrorType.BadRequest);

                    var test = new Test
                    {
                        CourseLesson = lesson,
                        Title = dto.Test.Title,
                        DurationInMinutes = dto.Test.DurationInMinutes,
                        TotalMark = dto.Test.Questions.Sum(q => q.Mark),
                        MaxAttempts = dto.Test.MaxAttempts,
                        Questions = dto.Test.Questions.Select((q, questionIndex) => new TestQuestion
                        {
                            QuestionText = q.Text,
                            Mark = q.Mark,
                            DisplayOrder = questionIndex + 1,
                            Choices = q.Choices.Select((c, choiceIndex) => new TestChoice
                            {
                                Text = c.Text,
                                IsCorrect = c.IsCorrect,
                                DisplayOrder = choiceIndex + 1
                            }).ToList()
                        }).ToList()
                    };
                    await _repository.AddTestAsync(test, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("CourseLessonService: Lesson with full content created.");
                return new GeneralResult(true, _messages.MsgLessonCreated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "CourseLessonService: Error creating lesson");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Create Lesson"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> UpdateLessonAsync(int lessonId, LessonUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var lesson = await _repository.GetByIdAsync(lessonId, cancellationToken);
                if (lesson == null)
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);

                if (dto.CourseId is not null && dto.CourseId != lesson.ProgramCourseId)
                {
                    if (!await _repository.CourseExistsAsync(dto.CourseId.Value, cancellationToken))
                        return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                    lesson.ProgramCourseId = dto.CourseId.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.Name)) lesson.Name = dto.Name;
                if (!string.IsNullOrWhiteSpace(dto.Description)) lesson.Description = dto.Description;
                if (dto.Order is not null) lesson.Order = dto.Order.Value;
                if (!string.IsNullOrWhiteSpace(dto.FileUrl)) lesson.FileUrl = dto.FileUrl;
                if (dto.DurationInMinutes is not null) lesson.DurationInMinutes = dto.DurationInMinutes.Value;

                lesson.UpdatedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgLessonUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CourseLessonService: Error updating lesson.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Updating Lesson"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<List<LessonFullDetailsDto>>> GetLessonsWithContentByCourseIdAsync(int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var lessons = await _repository.GetLessonsWithContentByCourseIdAsync(courseId, cancellationToken);
                if (!lessons.Any())
                    return new GeneralResult<List<LessonFullDetailsDto>>(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);

                var dtoList = lessons.Select(_queryService.MapToLessonDetailsDto).ToList();
                return new GeneralResult<List<LessonFullDetailsDto>>(true, _messages.MsgLessonRetrieved, dtoList, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CourseLessonService: Error retrieving lessons");
                return new GeneralResult<List<LessonFullDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Retrieving Lessons"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult> SoftDeleteLessonAsync(int lessonId, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var lesson = await _repository.GetLessonWithFullContentAsync(lessonId, cancellationToken);
                if (lesson == null)
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);

                var now = DateTimeOffset.UtcNow;
                lesson.IsDeleted = true;
                lesson.DeletedAt = now;

                foreach (var attachment in lesson.LessonAttachments)
                {
                    attachment.IsDeleted = true;
                    attachment.DeletedAt = now;
                }

                if (lesson.LessonTest != null)
                {
                    lesson.LessonTest.IsDeleted = true;
                    lesson.LessonTest.DeletedAt = now;
                    foreach (var question in lesson.LessonTest.Questions)
                    {
                        question.IsDeleted = true;
                        question.DeletedAt = now;
                        foreach (var choice in question.Choices)
                        {
                            choice.IsDeleted = true;
                            choice.DeletedAt = now;
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgLessonDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "CourseLessonService: Error during soft delete.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Deleting Lesson"), null, ErrorType.InternalServerError);
            }
        }
    }
}
