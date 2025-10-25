using Lumora.Interfaces.ProgramIntf;
using Lumora.Interfaces.QueriesIntf;
namespace Lumora.Services.Programs
{
    public class CourseLessonService(
            PgDbContext dbContext,
            IMapper mapper,
            TestMessage testMessage,
            ILogger<CourseLessonService> logger,
            CourseLessonMessages messages,
            IQueryService queryService) : ICourseLessonService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CourseLessonService> _logger = logger;
        private readonly CourseLessonMessages _messages = messages;
        private readonly IQueryService _queryService = queryService;

        /// <inheritdoc/>
        public async Task<GeneralResult> CreateLessonWithContentAsync(int courseId, LessonsWithContentCreateDto dto, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (dto == null)
                    return new GeneralResult(false, _messages.MsgNullOrEmpty, null, ErrorType.BadRequest);

                if (dto.Attachments?.Any() != true)
                    return new GeneralResult(false, _messages.MsgLessonMustHaveAttachment, null, ErrorType.BadRequest);

                var courseExists = await _dbContext.ProgramCourses.AnyAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);
                if (!courseExists)
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                var lesson = _mapper.Map<CourseLesson>(dto);
                lesson.ProgramCourseId = courseId;
                await _dbContext.CourseLessons.AddAsync(lesson, cancellationToken);

                var attachments = dto.Attachments.Select(attach =>
                {
                    var entity = _mapper.Map<LessonAttachment>(attach);
                    entity.CourseLesson = lesson;
                    return entity;
                }).ToList();

                await _dbContext.LessonAttachments.AddRangeAsync(attachments, cancellationToken);

                // Create test if provided
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

                    await _dbContext.Tests.AddAsync(test, cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync();
                _logger.LogInformation("CourseLessonService - CreateLessonWithContentAsync : Lesson with full content created successfully.");
                return new GeneralResult(true, _messages.MsgLessonCreated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CourseLessonService - CreateLessonWithContentAsync : Error creating lesson with content");
                await transaction.RollbackAsync();
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Create Lesson"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateLessonAsync(int lessonId, LessonUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var lesson = await _dbContext.CourseLessons.FirstOrDefaultAsync(
                    l => l.Id == lessonId && !l.IsDeleted,
                    cancellationToken);

                if (lesson == null)
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);

                if (dto.CourseId is not null && dto.CourseId != lesson.ProgramCourseId)
                {
                    var course = await _dbContext.ProgramCourses.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == dto.CourseId && !c.IsDeleted, cancellationToken);

                    if (course == null)
                    {
                        _logger.LogWarning("CourseLessonService - UpdateLessonAsync : Invalid CourseId {CourseId} provided.", dto.CourseId);
                        return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);
                    }

                    lesson.ProgramCourseId = dto.CourseId.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.Name))
                    lesson.Name = dto.Name;

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    lesson.Description = dto.Description;

                if (dto.Order is not null)
                    lesson.Order = dto.Order.Value;

                if (!string.IsNullOrWhiteSpace(dto.FileUrl))
                    lesson.FileUrl = dto.FileUrl;

                if (dto.DurationInMinutes is not null)
                    lesson.DurationInMinutes = dto.DurationInMinutes.Value;

                lesson.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("CourseLessonService - UpdateLessonAsync : Lesson updated successfully.");
                return new GeneralResult(true, _messages.MsgLessonUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CourseLessonService - UpdateLessonAsync : Error updating lesson.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Updating Lesson"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<LessonFullDetailsDto>>> GetLessonsWithContentByCourseIdAsync(int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var lessons = await _dbContext.CourseLessons.AsNoTracking()
                    .Include(l => l.LessonAttachments)
                    .Include(l => l.LessonTest)
                        .ThenInclude(e => e!.Questions)
                        .ThenInclude(e => e.Choices)
                    .Where(l => l.ProgramCourseId == courseId && !l.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!lessons.Any())
                    return new GeneralResult<List<LessonFullDetailsDto>>(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);

                var dtoList = lessons.Select(_queryService.MapToLessonDetailsDto).ToList();
                if (!dtoList.Any())
                    return new GeneralResult<List<LessonFullDetailsDto>>(false, _messages.MsgDataNotFound, null, ErrorType.NotFound);

                _logger.LogInformation("CourseLessonService - GetLessonsWithContentByCourseIdAsync : Lessons retrieved successfully.");
                return new GeneralResult<List<LessonFullDetailsDto>>(true, _messages.MsgLessonRetrieved, dtoList, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CourseLessonService - GetLessonsWithContentByCourseIdAsync : Error retrieving lessons");
                return new GeneralResult<List<LessonFullDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("Retrieving Lessons"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SoftDeleteLessonAsync(int lessonId, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var lesson = await _dbContext.CourseLessons
                    .Include(l => l.LessonAttachments)
                    .Include(l => l.LessonTest)
                        .ThenInclude(t => t!.Questions)
                        .ThenInclude(q => q.Choices)
                    .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, cancellationToken);

                if (lesson == null)
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);

                // Soft delete lesson
                lesson.IsDeleted = true;
                lesson.DeletedAt = DateTimeOffset.UtcNow;

                // Soft delete attachments
                foreach (var attachment in lesson.LessonAttachments)
                {
                    attachment.IsDeleted = true;
                    attachment.DeletedAt = DateTimeOffset.UtcNow;
                }

                // Soft delete test, questions, and choices
                if (lesson.LessonTest != null)
                {
                    lesson.LessonTest.IsDeleted = true;
                    lesson.LessonTest.DeletedAt = DateTimeOffset.UtcNow;

                    foreach (var question in lesson.LessonTest.Questions)
                    {
                        question.IsDeleted = true;
                        question.DeletedAt = DateTimeOffset.UtcNow;

                        foreach (var choice in question.Choices)
                        {
                            choice.IsDeleted = true;
                            choice.DeletedAt = DateTimeOffset.UtcNow;
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("CourseLessonService - SoftDeleteLessonAsync : Lesson and related content soft-deleted successfully.");
                return new GeneralResult(true, _messages.MsgLessonDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "CourseLessonService - SoftDeleteLessonAsync : Error during soft delete of lesson.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Deleting Lesson"), null, ErrorType.InternalServerError);
            }
        }
    }
}
