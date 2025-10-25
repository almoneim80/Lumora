namespace Lumora.Web.Validations.AuthenticationVal
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.PhoneNumber)
                .MustNotBeDefault()
                .WithMessage(messages.MsgPhoneRequired)
                .MustBeValidPhoneNumber(7, 15)
                .WithMessage(messages.MsgInvalidPhone);

            RuleFor(x => x.NewPassword)
                .MustNotBeDefault()
                .WithMessage(messages.MsgPasswordRequired)
                .Must(password =>
                    password.Length >= 8 &&
                    password.Any(char.IsUpper) &&
                    password.Any(char.IsLower) &&
                    password.Any(char.IsDigit) &&
                    password.Any(ch => !char.IsLetterOrDigit(ch)))
                .WithMessage(messages.MsgPasswordWeak);

            RuleFor(x => x.Token)
                .MustNotBeDefault()
                .WithMessage(messages.MsgTokenRequired);
        }
    }
}
