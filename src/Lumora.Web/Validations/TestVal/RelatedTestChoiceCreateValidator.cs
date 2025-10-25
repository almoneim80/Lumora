using Lumora.DTOs.Test;

namespace Lumora.Web.Validations.TestVal
{
    public class RelatedTestChoiceCreateValidator : AbstractValidator<RelatedTestChoiceDto>
    {
        public RelatedTestChoiceCreateValidator(TestMessage messages)
        {
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage(messages.MsgTestChoiceTextRequired);

            RuleFor(x => x.Text)
                .MaximumLength(250).WithMessage(messages.MsgTestChoiceTextTooLong);
        }
    }
}
