using Lumora.DTOs.Club;

namespace Lumora.Web.Validations.ClubVal
{
    public class AmbassadorAssignValidator : AbstractValidator<AmbassadorAssignDto>
    {
        public AmbassadorAssignValidator(ClubMessage messages)
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(messages.MsgUserIdRequired);

            RuleFor(x => x.DurationInDays)
                .GreaterThanOrEqualTo(1)
                .WithMessage(messages.MsgDurationMin)
                .LessThanOrEqualTo(365)
                .WithMessage(messages.MsgDurationMax);

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage(messages.MsgReasonTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));

            RuleFor(x => x.ClubPostId)
                .GreaterThan(0)
                .WithMessage(messages.MsgClubPostIdInvalid)
                .When(x => x.ClubPostId.HasValue);
        }
    }
}
