namespace Lumora.Web.Validations.AuthenticationVal
{
    public class Verify2FAValidator : AbstractValidator<Verify2FADto>
    {
        public Verify2FAValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.TwoFactorCode)
                .MustNotBeDefault()
                .WithMessage(messages.MsgTwoFactorCodeRequired);
        }
    }
}
