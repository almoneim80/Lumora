namespace Lumora.Web.Validations.AuthenticationVal
{
    public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailDto>
    {
        public ConfirmEmailValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.Token)
                .MustNotBeDefault()
                .WithMessage(messages.MsgTokenRequired);
        }
    }
}
