namespace Lumora.Web.Validations.Customer;
public class CompleteUserDetailValidator : AbstractValidator<CompleteUserDataFormDto>
{
    public CompleteUserDetailValidator(ILocalizationManager localizationManager)
    {
        RuleFor(x => x.DateOfBirth).MustNotBeDefault();
        RuleFor(x => x.DateOfBirth).MustBeValidDate(mustBePast: true);
        RuleFor(x => x.DateOfBirth).MustBeValidDate(minDate: DateTime.UtcNow.AddYears(-100), maxDate: DateTime.UtcNow.AddYears(-18))
            .WithMessage(localizationManager!.GetLocalizedStringWithReplaced("DateOfBirthInvalid"));

        RuleFor(x => x.AboutMe).MustNotBeDefault();
        RuleFor(x => x.AboutMe).MustBeValidDescription(minLength: 10, maxLength: 2000, allowSpecialCharacters: true);

        RuleFor(x => x.AvatarFile).MustNotBeDefault();
    }
}
