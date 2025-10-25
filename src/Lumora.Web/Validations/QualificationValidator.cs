using FluentValidation;

namespace Lumora.Validations;

public class QualificationCreateValidator : AbstractValidator<QualificationCreateDto>
{
    public QualificationCreateValidator()
    {
        RuleFor(x => x.UserId).MustNotBeDefault();

        RuleFor(x => x.Name).MustNotBeDefault();
        RuleFor(x => x.Name).MustContainOnlyLettersAndSpaces();
        RuleFor(x => x.Name).MustHaveLengthInRange(2, 250);

        RuleFor(x => x.DateEarned).MustNotBeDefault();
        RuleFor(x => x.DateEarned).MustBeValidDate();

        RuleFor(x => x.Description).MustNotBeDefault();
        RuleFor(x => x.Description).MustBeValidDescription(minLength: 10, maxLength: 2000, allowSpecialCharacters: true);
    }
}

public class QualificationUpdateValidator : AbstractValidator<QualificationUpdateDto>
{
    public QualificationUpdateValidator()
    {
        RuleFor(x => x.UserId).MustNotBeDefault();

        //RuleFor(x => x.Name).MustContainOnlyLettersAndSpaces();
        //RuleFor(x => x.Name).MustHaveLengthInRange(2, 250);

        //RuleFor(x => x.DateEarned).MustBeValidDate();
        //RuleFor(x => x.DateEarned).MustBeValidDate(mustBeFuture: true);

        //RuleFor(x => x.Description).MustBeValidDescription(minLength: 10, maxLength: 2000, allowSpecialCharacters: true);
    }
}
