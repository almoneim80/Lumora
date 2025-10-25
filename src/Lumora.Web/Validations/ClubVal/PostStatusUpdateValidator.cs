using Lumora.DTOs.Club;

namespace Lumora.Web.Validations.ClubVal
{
    public class PostStatusUpdateValidator : AbstractValidator<PostStatusUpdateDto>
    {
        public PostStatusUpdateValidator(ClubMessage messages)
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0)
                .WithMessage(messages.MsgPostIdRequired);

            RuleFor(x => x.NewStatus)
                .IsInEnum()
                .WithMessage(messages.MsgInvalidPostStatus);

            When(x => x.NewStatus == ClubPostStatus.Rejected, () =>
            {
                RuleFor(x => x.RejectionReason)
                    .NotEmpty()
                    .WithMessage(messages.MsgRejectionReasonRequired)
                    .MaximumLength(500)
                    .WithMessage(messages.MsgRejectionReasonTooLong);
            });
        }
    }
}
