namespace Lumora.Web.Validations.AuthenticationVal
{
    public class ChangePhoneNumberValidator : AbstractValidator<ChangePhoneNumberDto>
    {
        public ChangePhoneNumberValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.PhoneNumber)
                .MustBeValidPhoneNumber(7, 15)
                .WithMessage(messages.MsgInvalidPhone);
        }
    }
}
