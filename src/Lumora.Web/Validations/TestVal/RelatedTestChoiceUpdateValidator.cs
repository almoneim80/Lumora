using Lumora.DTOs.Test;

namespace Lumora.Web.Validations.TestVal
{
    public class RelatedTestChoiceUpdateValidator : AbstractValidator<RelatedTestChoiceUpdateDto>
    {
        public RelatedTestChoiceUpdateValidator(TestMessage messages)
        {
            When(x => !string.IsNullOrWhiteSpace(x.Text), () =>
            {
                RuleFor(x => x.Text!)
                    .NotEmpty().WithMessage(messages.MsgTestChoiceTextRequired);

                RuleFor(x => x.Text!)
                    .MaximumLength(300).WithMessage(messages.MsgTestChoiceTextTooLong);
            });
        }
    }
}
