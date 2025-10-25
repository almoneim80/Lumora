using Lumora.DTOs.Test;

namespace Lumora.Web.Validations.TestVal
{
    public class RelatedTestQuestionUpdateValidator : AbstractValidator<RelatedTestQuestionUpdateDto>
    {
        public RelatedTestQuestionUpdateValidator(TestMessage messages)
        {
            When(x => !string.IsNullOrWhiteSpace(x.Question), () =>
            {
                RuleFor(x => x.Question!)
                    .NotEmpty().WithMessage(messages.MsgTestQuestionRequired);

                RuleFor(x => x.Question!)
                    .MaximumLength(500).WithMessage(messages.MsgTestQuestionTooLong);
            });

            When(x => x.Mark.HasValue, () =>
            {
                RuleFor(x => x.Mark!.Value)
                    .GreaterThan(0).WithMessage(messages.MsgTestQuestionMarkInvalid);
            });

            When(x => x.Choices is not null && x.Choices.Any(), () =>
            {
                RuleForEach(x => x.Choices!)
                    .SetValidator(new RelatedTestChoiceUpdateValidator(messages));

                RuleFor(x => x.Choices!.Count(c => c.IsCorrect == true))
                    .GreaterThan(0)
                    .WithMessage(messages.MsgTestQuestionMustHaveCorrectChoice);
            });
        }
    }
}
