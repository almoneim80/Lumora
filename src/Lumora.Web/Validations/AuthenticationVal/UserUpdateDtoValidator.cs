using Quartz.Util;

namespace Lumora.Web.Validations.AuthenticationVal
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateFormDto>
    {
        public UserUpdateDtoValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.FullName)
                .MustContainOnlyLettersAndSpaces()
                .WithMessage(messages.MsgFullNameLettersOnly)
                .MustHaveLengthInRange(2, 250)
                .When(x => !string.IsNullOrWhiteSpace(x.FullName));

            RuleFor(x => x.City)
                .MustContainOnlyLettersAndSpaces()
                .WithMessage(messages.MsgCityLettersOnly)
                .MustHaveLengthInRange(2, 250)
                .When(x => !string.IsNullOrWhiteSpace(x.City));

            RuleFor(x => x.Sex)
                .MustContainOnlyLettersAndSpaces()
                .WithMessage(messages.MsgSexLettersOnly)
                .MustHaveLengthInRange(2, 250)
                .When(x => !string.IsNullOrWhiteSpace(x.Sex));

            RuleFor(x => x.AboutMe)
                .MustBeValidDescription(5, 500, allowSpecialCharacters: true)
                .WithMessage(messages.MsgAboutMeInvalid)
                .When(x => !string.IsNullOrWhiteSpace(x.AboutMe));

            RuleFor(x => x.DateOfBirth)
                .MustBeValidDate(maxDate: DateTimeOffset.UtcNow.AddYears(-18), mustBePast: true)
                .WithMessage(messages.MsgDateOfBirthInvalid)
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
