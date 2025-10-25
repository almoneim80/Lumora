using FluentValidation;
namespace Lumora.Validations;

public class TouristClubSubscriberCreateValidator : AbstractValidator<TouristClubSubscriberCreateDto>
{
    public TouristClubSubscriberCreateValidator()
    {
        RuleFor(x => x.EventId).MustNotBeDefault();

        RuleFor(x => x.UserId).MustNotBeDefault();

        //RuleFor(x => x.SubscriptionDate).MustNotBeDefault();
        //RuleFor(x => x.SubscriptionDate).MustBeValidDate();
    }
}
