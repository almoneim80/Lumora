namespace Lumora.Application.Validations.TestVal
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
