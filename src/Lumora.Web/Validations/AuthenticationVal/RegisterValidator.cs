namespace Lumora.Web.Validations.AuthenticationVal
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.FullName)
                .MustNotBeDefault()
                .WithMessage(messages.MsgFullNameRequired);

            RuleFor(x => x.FullName)
                .MustContainOnlyLettersAndSpaces()
                .WithMessage(messages.MsgFullNameLettersOnly)
                .MustHaveLengthInRange(2, 250);

            RuleFor(x => x.City)
                .MustNotBeDefault()
                .WithMessage(messages.MsgCityRequired);

            RuleFor(x => x.City)
                .MustContainOnlyLettersAndSpaces()
                .WithMessage(messages.MsgCityLettersOnly)
                .MustHaveLengthInRange(2, 250);

            RuleFor(x => x.Sex)
                .MustNotBeDefault()
                .WithMessage(messages.MsgSexRequired)
                .MustContainOnlyLettersAndSpaces()
                .WithMessage(messages.MsgSexLettersOnly)
                .MustHaveLengthInRange(2, 250);

            RuleFor(x => x.PhoneNumber)
                .MustNotBeDefault()
                .WithMessage(messages.MsgPhoneRequired)
                .MustBeValidPhoneNumber(7, 15)
                .WithMessage(messages.MsgInvalidPhone);

            RuleFor(x => x.Email)
                .MustNotBeDefault()
                .WithMessage(messages.MsgEmailRequired)
                .Matches("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")
                .WithMessage(messages.MsgInvalidEmail);

            RuleFor(x => x.Password)
                .MustNotBeDefault()
                .WithMessage(messages.MsgPasswordRequired)
                .Must(password => password.Length >= 8 &&
                                   password.Any(char.IsUpper) &&
                                   password.Any(char.IsLower) &&
                                   password.Any(char.IsDigit) &&
                                   password.Any(ch => !char.IsLetterOrDigit(ch)))
                .WithMessage(messages.MsgPasswordWeak);

            RuleFor(x => x.ConfirmPassword)
                .MustNotBeDefault()
                .WithMessage(messages.MsgConfirmPasswordRequired)
                .Equal(x => x.Password)
                .WithMessage(messages.MsgPasswordMismatch);
        }
    }
}
