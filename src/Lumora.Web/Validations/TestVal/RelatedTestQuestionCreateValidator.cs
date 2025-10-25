using Lumora.DTOs.Test;

namespace Lumora.Web.Validations.TestVal
{
    public class RelatedTestQuestionCreateValidator : AbstractValidator<RelatedTestQuestionDto>
    {
        public RelatedTestQuestionCreateValidator(TestMessage messages)
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage(messages.MsgTestQuestionTextRequired);

            RuleFor(x => x.Text)
                .MaximumLength(500).WithMessage(messages.MsgTestQuestionTextTooLong);

            RuleFor(x => x.Mark)
                .GreaterThan(0).WithMessage(messages.MsgTestQuestionMarkInvalid);

            RuleFor(x => x.Choices)
                .NotEmpty().WithMessage(messages.MsgTestQuestionChoicesRequired);

            RuleFor(x => x.Choices.Count(c => c.IsCorrect))
                .GreaterThan(0).WithMessage(messages.MsgTestQuestionNoCorrectChoice);
        }
    }
}
