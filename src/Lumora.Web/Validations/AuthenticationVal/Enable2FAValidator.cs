namespace Lumora.Web.Validations.AuthenticationVal
{
    public class Enable2FAValidator : AbstractValidator<Enable2FADto>
    {
        public Enable2FAValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.AppVerificationCode)
                .MustNotBeDefault()
                .WithMessage(messages.MsgAppVerificationCodeRequired);
        }
    }
}
