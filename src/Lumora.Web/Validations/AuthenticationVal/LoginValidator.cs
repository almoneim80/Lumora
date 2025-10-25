namespace Lumora.Web.Validations.AuthenticationVal
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.PhoneNumber)
                .MustNotBeDefault()
                .WithMessage(messages.MsgPhoneRequired)
                .MustBeValidPhoneNumber(7, 15)
                .WithMessage(messages.MsgInvalidPhone);

            RuleFor(x => x.Password)
                .MustNotBeDefault()
                .WithMessage(messages.MsgPasswordRequired);
        }
    }
}
