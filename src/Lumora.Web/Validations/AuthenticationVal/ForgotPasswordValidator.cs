namespace Lumora.Web.Validations.AuthenticationVal
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.PhoneNumber)
                .MustNotBeDefault()
                .WithMessage(messages.MsgPhoneRequired)
                .MustBeValidPhoneNumber(7, 15)
                .WithMessage(messages.MsgInvalidPhone);
        }
    }
}
