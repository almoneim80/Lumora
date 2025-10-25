using FluentValidation;

namespace Lumora.Validations;

public class NotificationCreateValidator : AbstractValidator<NotificationCreateDto>
{
    public NotificationCreateValidator()
    {
        RuleFor(x => x.ReciverId).MustNotBeDefault();

        RuleFor(x => x.SenderId).MustNotBeDefault();

        RuleFor(x => x.Message).MustNotBeDefault();
        RuleFor(x => x.Message).MustHaveLengthInRange(2, 1000);

        RuleFor(x => x.ReadAt).MustBeValidDate(mustBeFuture: true);
    }
}

public class NotificationUpdateValidator : AbstractValidator<NotificationUpdateDto>
{
    public NotificationUpdateValidator()
    {
        RuleFor(x => x.ReciverId).MustNotBeDefault();

        //RuleFor(x => x.SenderId).MustNotBeDefault();

        //RuleFor(x => x.Message).MustHaveLengthInRange(2, 1000);

        //RuleFor(x => x.ReadAt).MustBeValidDate(mustBeFuture: true);
    }
}
