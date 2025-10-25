using Lumora.DTOs.Test;

namespace Lumora.Web.Validations.TestVal
{
    public class TestCreateValidator : AbstractValidator<TestCreateDto>
    {
        public TestCreateValidator(TestMessage messages)
        {
            RuleFor(x => x.LessonId)
                .GreaterThan(0).WithMessage(messages.MsgTestLessonIdRequired);

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(messages.MsgTestTitleRequired);

            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage(messages.MsgTestTitleTooLong);

            RuleFor(x => x.DurationInMinutes)
                .GreaterThan(0).WithMessage(messages.MsgTestDurationInvalid);

            RuleFor(x => x.TotalMark)
                .GreaterThan(0).WithMessage(messages.MsgTestTotalMarkInvalid);

            RuleFor(x => x.MaxAttempts)
                .GreaterThan(0).WithMessage(messages.MsgTestMaxAttemptsInvalid);

            RuleFor(x => x.Questions)
                .NotEmpty().WithMessage(messages.MsgTestQuestionsRequired);
        }
    }
}
