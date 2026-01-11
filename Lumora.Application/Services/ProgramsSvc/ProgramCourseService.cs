namespace Lumora.Application.Services.Programs
{
    public class ProgramCourseService(
        IProgramCourseRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProgramCourseService> logger,
        CourseMessage messages,
        IQueryService queryService) : IProgramCourseService
    {
        private readonly IProgramCourseRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<ProgramCourseService> _logger = logger;
        private readonly CourseMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> CreateCourseWithContentAsync(CourseWithLessonsCreateDto dto, CancellationToken cancellationToken)
        {
            // استخدام UnitOfWork لإدارة العملية (Transaction)
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                if (dto.Lessons?.Any() != true)
                {
                    _logger.LogInformation("ProgramCourseService - CreateCourseWithContentAsync : No lessons found.");
                    return new GeneralResult(false, _messages.MsgNoLessons, null, ErrorType.BadRequest);
                }

                var course = _mapper.Map<ProgramCourse>(dto);
                await _repository.AddCourseAsync(course, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken); // نحتاج الـ ID للكورس

                var lessons = new List<CourseLesson>();
                var attachments = new List<LessonAttachment>();
                var tests = new List<Test>();
                var questions = new List<TestQuestion>();
                var choices = new List<TestChoice>();

                foreach (var lessonDto in dto.Lessons)
                {
                    var lesson = _mapper.Map<CourseLesson>(lessonDto);
                    lesson.ProgramCourseId = course.Id;
                    lessons.Add(lesson);

                    if (lessonDto.Attachments?.Any() != true)
                    {
                        return new GeneralResult(false, _messages.GetNoLessonAttachment(lesson.Name), null, ErrorType.BadRequest);
                    }

                    foreach (var attach in lessonDto.Attachments)
                    {
                        var attachment = _mapper.Map<LessonAttachment>(attach);
                        attachment.CourseLesson = lesson;
                        attachments.Add(attachment);
                    }

                    if (lessonDto.Test != null)
                    {
                        var testDto = lessonDto.Test;
                        var test = new Test { CourseLesson = lesson, DurationInMinutes = testDto.DurationInMinutes, Title = testDto.Title, TotalMark = testDto.TotalMark };
                        tests.Add(test);

                        int displayOrder = 1;
                        foreach (var questionDto in testDto.Questions)
                        {
                            var question = new TestQuestion { QuestionText = questionDto.Text, Mark = questionDto.Mark, Test = test, DisplayOrder = displayOrder++ };
                            questions.Add(question);

                            int choiceOrder = 1;
                            foreach (var choiceDto in questionDto.Choices)
                            {
                                choices.Add(new TestChoice { Text = choiceDto.Text, IsCorrect = choiceDto.IsCorrect, TestQuestion = question, DisplayOrder = choiceOrder++ });
                            }
                        }
                    }
                }

                await _repository.AddLessonsRangeAsync(lessons, cancellationToken);
                await _repository.AddAttachmentsRangeAsync(attachments, cancellationToken);
                await _repository.AddTestsRangeAsync(tests, cancellationToken);
                await _repository.AddQuestionsRangeAsync(questions, cancellationToken);
                await _repository.AddChoicesRangeAsync(choices, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgCourseCreated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course with content");
                await transaction.RollbackAsync(cancellationToken);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("creating course"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateCourseAsync(int courseId, CourseUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _repository.GetByIdAsync(courseId, cancellationToken);
                if (course == null) return new GeneralResult(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                if (dto.ProgramId != null)
                {
                    var program = await _repository.GetProgramByIdAsync(dto.ProgramId.Value, cancellationToken);
                    if (program == null) return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                    course.ProgramId = program.Id;
                }

                if (dto.Name != null) course.Name = dto.Name;
                if (dto.Description != null) course.Description = dto.Description;
                if (dto.Order.HasValue) course.Order = dto.Order.Value;
                if (dto.Logo != null) course.Logo = dto.Logo;

                course.UpdatedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgCourseUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating course"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<CourseFullDetailsDto>> GetCourseWithContentByIdAsync(int courseId, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _repository.GetByIdAsync(courseId, cancellationToken, asNoTracking: true);
                if (course == null) return new GeneralResult<CourseFullDetailsDto>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                var data = queryService.MapToCourseDetailsDto(course);
                return new GeneralResult<CourseFullDetailsDto>(true, _messages.MsgDataRetrieved, data, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course details");
                return new GeneralResult<CourseFullDetailsDto>(false, _messages.GetUnexpectedErrorMessage("Retrieving course details"), null, ErrorType.InternalServerError);
            }
        }
    }
}
