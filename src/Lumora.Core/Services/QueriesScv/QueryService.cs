using Lumora.DTOs.Test;
using Lumora.Interfaces.QueriesIntf;
namespace Lumora.Services.QueriesScv
{
    public class QueryService : IQueryService
    {
        public TrainingProgramFullDetailsDto MapToProgramDetailsDto(TrainingProgram program)
        {
            var now = DateTimeOffset.UtcNow;
            return new TrainingProgramFullDetailsDto
            {
                Id = program.Id,
                Name = program.Name,
                Description = program.Description,
                Price = program.Price ?? 0,
                Discount = program.Discount ?? 0,
                Logo = program.Logo,
                HasCertificateExpiration = program.HasCertificateExpiration,
                CertificateValidityInMonth = program.CertificateValidityInMonth,
                ProgramCourses = program.ProgramCourses?.Where(c => !c.IsDeleted)
                    .Select(c => new CourseFullDetailsDto
                    {
                        ProgramId = c.ProgramId,
                        CourseId = c.Id,
                        ProgramName = c.TrainingProgram?.Name,
                        CourseName = c.Name,
                        CourseDescription = c.Description ?? string.Empty,
                        CourseOrder = c.Order,
                        CourseLogo = c.Logo,
                        CreatedAt = c.CreatedAt ?? now,
                        Lessons = c.Lessons?
                    .Where(l => !l.IsDeleted)
                    .Select(l => new LessonFullDetailsDto
                    {
                        LessonId = l.Id,
                        LessonName = l.Name,
                        LessonDescription = l.Description,
                        LessonDuration = l.DurationInMinutes,
                        LessonFileUrl = l.FileUrl,
                        LessonOrder = l.Order,
                        CreatedAt = l.CreatedAt ?? now,
                        Attachments = l.LessonAttachments?
                            .Where(a => !a.IsDeleted)
                            .Select(a => new AttachmentDetailsDto
                            {
                                AttachmentId = a.Id,
                                AttachmentFileUrl = a.FileUrl,
                                AttachmentOpenCount = a.OpenCount,
                                CreatedAt = a.CreatedAt ?? now
                            }).ToList() ?? new(),
                        Test = l.LessonTest != null && !l.LessonTest.IsDeleted ?
                        new TestDetailsDto
                        {
                            Id = l.Id,
                            LessonId = l.LessonTest.Id,
                            LessonName = l.Name,
                            Title = l.LessonTest.Title,
                            DurationInMinutes = l.LessonTest.DurationInMinutes,
                            TotalMark = l.LessonTest.TotalMark,
                            MaxAttempts = l.LessonTest.MaxAttempts,
                            Questions = l.LessonTest.Questions?
                                .Where(q => !q.IsDeleted)
                            .Select(q => new RelatedTestQuestionDetailsDto
                            {
                                Id = q.Id,
                                Question = q.QuestionText,
                                Mark = q.Mark,
                                DisplayOrder = q.DisplayOrder,
                                Choices = q.Choices.Where(c => !c.IsDeleted)
                                .Select(c => new RelatedTestChoiceDetailsDto
                                {
                                    Id = c.Id,
                                    Text = c.Text,
                                    IsCorrect = c.IsCorrect,
                                    DisplayOrder = c.DisplayOrder
                                }).ToList()
                            }).ToList() ?? new()
                        }
                        : null
                    }).ToList() ?? new(),
                    }).ToList()
            };
        }

        public CourseFullDetailsDto MapToCourseDetailsDto(ProgramCourse course)
        {
            var now = DateTimeOffset.UtcNow;
            return new CourseFullDetailsDto
            {
                ProgramId = course.ProgramId,
                CourseId = course.Id,
                ProgramName = course.TrainingProgram?.Name ?? string.Empty,
                CourseName = course.Name,
                CourseDescription = course.Description ?? string.Empty,
                CourseOrder = course.Order,
                CourseLogo = course.Logo,
                CreatedAt = course.CreatedAt ?? now,
                Lessons = course.Lessons?
                    .Where(l => !l.IsDeleted)
                    .Select(l => new LessonFullDetailsDto
                    {
                        LessonId = l.Id,
                        LessonName = l.Name,
                        LessonDescription = l.Description,
                        LessonDuration = l.DurationInMinutes,
                        LessonFileUrl = l.FileUrl,
                        LessonOrder = l.Order,
                        CreatedAt = l.CreatedAt ?? now,
                        Attachments = l.LessonAttachments?
                            .Where(a => !a.IsDeleted)
                            .Select(a => new AttachmentDetailsDto
                            {
                                AttachmentId = a.Id,
                                AttachmentFileUrl = a.FileUrl,
                                AttachmentOpenCount = a.OpenCount,
                                CreatedAt = a.CreatedAt ?? now
                            }).ToList() ?? new(),
                        Test = l.LessonTest != null && !l.LessonTest.IsDeleted ?
                        new TestDetailsDto
                        {
                            Id = l.Id,
                            LessonId = l.LessonTest.Id,
                            LessonName = l.Name,
                            Title = l.LessonTest.Title,
                            DurationInMinutes = l.LessonTest.DurationInMinutes,
                            TotalMark = l.LessonTest.TotalMark,
                            MaxAttempts = l.LessonTest.MaxAttempts,
                            Questions = l.LessonTest.Questions?
                                .Where(q => !q.IsDeleted)
                            .Select(q => new RelatedTestQuestionDetailsDto
                            {
                                Id = q.Id,
                                Question = q.QuestionText,
                                Mark = q.Mark,
                                DisplayOrder = q.DisplayOrder,
                                Choices = q.Choices.Where(c => !c.IsDeleted)
                                .Select(c => new RelatedTestChoiceDetailsDto
                                {
                                    Id = c.Id,
                                    Text = c.Text,
                                    IsCorrect = c.IsCorrect,
                                    DisplayOrder = c.DisplayOrder
                                }).ToList()
                            }).ToList() ?? new()
                        }
                        : null
                    }).ToList() ?? new()
            };
        }

        public LessonFullDetailsDto MapToLessonDetailsDto(CourseLesson lesson)
        {
            var now = DateTimeOffset.UtcNow;
            return new LessonFullDetailsDto
            {
                LessonId = lesson.Id,
                LessonName = lesson.Name!,
                LessonFileUrl = lesson.FileUrl!,
                LessonOrder = lesson.Order,
                LessonDuration = lesson.DurationInMinutes,
                LessonDescription = lesson.Description!,
                CreatedAt = lesson.CreatedAt ?? now,
                Attachments = lesson.LessonAttachments?
                    .Where(a => !a.IsDeleted)
                    .Select(a => new AttachmentDetailsDto
                    {
                        AttachmentId = a.Id,
                        AttachmentFileUrl = a.FileUrl!,
                        AttachmentOpenCount = a.OpenCount,
                        CreatedAt = a.CreatedAt ?? now,
                    }).ToList(),
                Test = lesson.LessonTest != null && !lesson.LessonTest.IsDeleted ?
                        new TestDetailsDto
                        {
                            Id = lesson.LessonTest.Id,
                            LessonId = lesson.LessonTest.Id,
                            Title = lesson.LessonTest.Title,
                            LessonName = lesson.Name,
                            DurationInMinutes = lesson.LessonTest.DurationInMinutes,
                            TotalMark = lesson.LessonTest.TotalMark,
                            MaxAttempts = lesson.LessonTest.MaxAttempts,
                            Questions = lesson.LessonTest.Questions?
                                .Where(q => !q.IsDeleted)
                            .Select(q => new RelatedTestQuestionDetailsDto
                            {
                                Id = q.Id,
                                Question = q.QuestionText,
                                Mark = q.Mark,
                                DisplayOrder = q.DisplayOrder,
                                Choices = q.Choices.Where(c => !c.IsDeleted)
                                .Select(c => new RelatedTestChoiceDetailsDto
                                {
                                    Text = c.Text,
                                    IsCorrect = c.IsCorrect,
                                    DisplayOrder = c.DisplayOrder
                                }).ToList()
                            }).ToList() ?? new()
                        }
                        : null
            };
        }

        public TestDetailsDto MapToTestDetailsDto(Test test)
        {
            return new TestDetailsDto
            {
                Id = test.Id,
                LessonId = test.Id,
                LessonName = test.CourseLesson.Name,
                Title = test.Title,
                DurationInMinutes = test.DurationInMinutes,
                TotalMark = test.TotalMark,
                MaxAttempts = test.MaxAttempts,
                Questions = test.Questions?
                .Where(q => !q.IsDeleted)
                .Select(q => new RelatedTestQuestionDetailsDto
                {
                    Id = q.Id,
                    Question = q.QuestionText,
                    Mark = q.Mark,
                    DisplayOrder = q.DisplayOrder,
                    Choices = q.Choices.Where(c => !c.IsDeleted)
                    .Select(c => new RelatedTestChoiceDetailsDto
                    {
                        Id = c.Id,
                        Text = c.Text,
                        IsCorrect = c.IsCorrect,
                        DisplayOrder = c.DisplayOrder
                    }).ToList()
                }).ToList() ?? new()
            };
        }
    }
}
