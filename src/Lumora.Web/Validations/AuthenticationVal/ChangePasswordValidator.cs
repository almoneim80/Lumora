namespace Lumora.Web.Validations.AuthenticationVal
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.CurrentPassword)
                .MustNotBeDefault()
                .WithMessage(messages.MsgCurrentPasswordRequired);

            RuleFor(x => x.NewPassword)
                .MustNotBeDefault()
                .WithMessage(messages.MsgNewPasswordRequired)
                .Must(password =>
                    password.Length >= 8 &&
                    password.Any(char.IsUpper) &&
                    password.Any(char.IsLower) &&
                    password.Any(char.IsDigit) &&
                    password.Any(ch => !char.IsLetterOrDigit(ch)))
                .WithMessage(messages.MsgPasswordWeak);

            RuleFor(x => x.ConfirmPassword)
                .MustNotBeDefault()
                .WithMessage(messages.MsgConfirmPasswordRequired)
                .Equal(x => x.NewPassword)
                .WithMessage(messages.MsgPasswordMismatch);
        }
    }
}
