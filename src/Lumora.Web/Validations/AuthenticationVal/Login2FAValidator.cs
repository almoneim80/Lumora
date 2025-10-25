namespace Lumora.Web.Validations.AuthenticationVal
{
    public class Login2FAValidator : AbstractValidator<Login2FADto>
    {
        public Login2FAValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.VerificationCode)
                .MustNotBeDefault()
                .WithMessage(messages.MsgVerificationCodeRequired);
        }
    }
}
