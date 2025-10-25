using Lumora.DTOs.Test;

namespace Lumora.Web.Validations.TestVal
{
    public class TestUpdateValidator : AbstractValidator<TestUpdateDto>
    {
        public TestUpdateValidator(TestMessage messages)
        {
            When(x => !string.IsNullOrWhiteSpace(x.Title), () =>
            {
                RuleFor(x => x.Title!)
                    .NotEmpty().WithMessage(messages.MsgTestTitleRequired);

                RuleFor(x => x.Title!)
                    .MaximumLength(250).WithMessage(messages.MsgTestTitleTooLong);
            });

            When(x => x.DurationInMinutes.HasValue, () =>
            {
                RuleFor(x => x.DurationInMinutes!.Value)
                    .GreaterThan(0).WithMessage(messages.MsgTestDurationInvalid);
            });

            When(x => x.TotalMark.HasValue, () =>
            {
                RuleFor(x => x.TotalMark!.Value)
                    .GreaterThan(0).WithMessage(messages.MsgTestTotalMarkInvalid);
            });

            When(x => x.MaxAttempts.HasValue, () =>
            {
                RuleFor(x => x.MaxAttempts!.Value)
                    .GreaterThan(0).WithMessage(messages.MsgTestMaxAttemptsInvalid);
            });

            When(x => x.Questions is not null && x.Questions.Any(), () =>
            {
                RuleForEach(x => x.Questions!)
                    .SetValidator(new RelatedTestQuestionUpdateValidator(messages));
            });
        }
    }
}
