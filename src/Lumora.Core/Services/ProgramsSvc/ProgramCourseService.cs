using Lumora.Interfaces.ProgramIntf;
using Lumora.Interfaces.QueriesIntf;

namespace Lumora.Services.Programs
{
    public class ProgramCourseService(PgDbContext dbContext, IMapper mapper, ILogger<ProgramCourseService> logger, CourseMessage messages, IQueryService queryService) : IProgramCourseService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<ProgramCourseService> _logger = logger;
        private readonly CourseMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> CreateCourseWithContentAsync(CourseWithLessonsCreateDto dto, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if (dto.Lessons?.Any() != true)
                {
                    _logger.LogInformation("ProgramCourseService - CreateCourseWithContentAsync : No lessons found, Course should have at least one lesson.");
                    return new GeneralResult(false, _messages.MsgNoLessons, null, ErrorType.BadRequest);
                }

                // Create the course
                var course = _mapper.Map<ProgramCourse>(dto);
                await _dbContext.ProgramCourses.AddAsync(course, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var lessons = new List<CourseLesson>();
                var attachments = new List<LessonAttachment>();
                var tests = new List<Test>();
                var questions = new List<TestQuestion>();
                var choices = new List<TestChoice>();

                foreach (var lessonDto in dto.Lessons)
                {
                    // Create lesson
                    var lesson = _mapper.Map<CourseLesson>(lessonDto);
                    lesson.ProgramCourseId = course.Id;
                    lessons.Add(lesson);

                    if (lessonDto.Attachments?.Any() != true)
                    {
                        _logger.LogWarning("ProgramCourseService - CreateCourseWithContentAsync : No attachments for lesson '{LessonName}'.", lesson.Name);
                        return new GeneralResult(false, _messages.GetNoLessonAttachment(lesson.Name), null, ErrorType.BadRequest);
                    }

                    foreach (var attach in lessonDto.Attachments)
                    {
                        var attachment = _mapper.Map<LessonAttachment>(attach);
                        attachment.CourseLesson = lesson;
                        attachments.Add(attachment);
                    }

                    // Optional Test
                    if (lessonDto.Test != null)
                    {
                        var testDto = lessonDto.Test;

                        if (testDto.Questions?.Any() != true)
                        {
                            _logger.LogWarning("ProgramCourseService - CreateCourseWithContentAsync : Test for lesson '{LessonName}' has no questions.", lesson.Name);
                            return new GeneralResult(false, _messages.MsgTestMustHaveQuestions, null, ErrorType.BadRequest);
                        }

                        var test = new Test
                        {
                            CourseLesson = lesson,
                            DurationInMinutes = testDto.DurationInMinutes,
                            Title = testDto.Title,
                            TotalMark = testDto.TotalMark
                        };
                        tests.Add(test);

                        int displayOrder = 1;
                        foreach (var questionDto in testDto.Questions)
                        {
                            if (questionDto.Choices?.Count < 2)
                            {
                                _logger.LogWarning("ProgramCourseService - CreateCourseWithContentAsync : Question in lesson '{LessonName}' has less than two choices.", lesson.Name);
                                return new GeneralResult(false, _messages.GetNoExercisechoices(lesson.Name), null, ErrorType.BadRequest);
                            }

                            var question = new TestQuestion
                            {
                                QuestionText = questionDto.Text,
                                Mark = questionDto.Mark,
                                Test = test,
                                DisplayOrder = displayOrder++
                            };
                            questions.Add(question);

                            if (questionDto?.Choices != null)
                            {
                                int choiceOrder = 1;
                                foreach (var choiceDto in questionDto.Choices)
                                {
                                    var choice = new TestChoice
                                    {
                                        Text = choiceDto.Text,
                                        IsCorrect = choiceDto.IsCorrect,
                                        TestQuestion = question,
                                        DisplayOrder = choiceOrder++
                                    };
                                    choices.Add(choice);
                                }
                            }
                        }
                    }
                }

                await _dbContext.CourseLessons.AddRangeAsync(lessons, cancellationToken);
                await _dbContext.LessonAttachments.AddRangeAsync(attachments, cancellationToken);
                await _dbContext.Tests.AddRangeAsync(tests, cancellationToken);
                await _dbContext.TestQuestions.AddRangeAsync(questions, cancellationToken);
                await _dbContext.TestChoices.AddRangeAsync(choices, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("ProgramCourseService - CreateCourseWithContentAsync: Course with full content created successfully.");
                return new GeneralResult(true, _messages.MsgCourseCreated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgramCourseService - CreateCourseWithContentAsync : Error creating course with content");
                await transaction.RollbackAsync();
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("creating course"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateCourseAsync(int courseId, CourseUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("ProgramCourseService - UpdateCourseAsync: Received null DTO.");
                    return new GeneralResult(false, _messages.MsgNullOrEmpty, null, ErrorType.BadRequest);
                }

                var course = await _dbContext.ProgramCourses.FirstOrDefaultAsync(pc => pc.Id == courseId && !pc.IsDeleted, cancellationToken);
                if (course == null || course.IsDeleted)
                    return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                if (dto.ProgramId != null)
                {
                    var program = await _dbContext.TrainingPrograms
                        .FirstOrDefaultAsync(p => p.Id == dto.ProgramId && !p.IsDeleted, cancellationToken);

                    if (program == null)
                    {
                        _logger.LogError("ProgramCourseService - UpdateCourseAsync : Program with ID {ProgramId} not found or deleted.", dto.ProgramId);
                        return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                    }

                    course.ProgramId = program.Id;
                }

                if (dto.Name != null) course.Name = dto.Name;
                if (dto.Description != null) course.Description = dto.Description;
                if (dto.Order.HasValue) course.Order = dto.Order.Value;
                if (dto.Logo != null) course.Logo = dto.Logo;
                course.UpdatedAt = DateTimeOffset.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("ProgramCourseService - UpdateCourseAsync : Course updated successfully.");
                return new GeneralResult(true, _messages.MsgCourseUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgramCourseService - UpdateCourseAsync : Error updating course.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating course"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<CourseFullDetailsDto>> GetCourseWithContentByIdAsync(int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _dbContext.ProgramCourses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted, cancellationToken);
                if (course == null) return new GeneralResult<CourseFullDetailsDto>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                var data = queryService.MapToCourseDetailsDto(course);
                if (data == null) return new GeneralResult<CourseFullDetailsDto>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                _logger.LogInformation("ProgramCourseService - GetCourseWithContentByIdAsync : Course details retrieved successfully.");
                return new GeneralResult<CourseFullDetailsDto>(true, _messages.MsgDataRetrieved, data, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProgramCourseService - GetCourseWithContentByIdAsync : Error retrieving course details");
                return new GeneralResult<CourseFullDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("Retrieving course details"), null, ErrorType.InternalServerError);
            }
        }
    }
}
