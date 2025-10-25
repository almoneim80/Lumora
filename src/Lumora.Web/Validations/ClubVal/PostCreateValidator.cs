using Quartz.Util;
using Lumora.DTOs.Club;

namespace Lumora.Web.Validations.ClubVal
{
    public class PostCreateValidator : AbstractValidator<PostCreateDto>
    {
        public PostCreateValidator(ClubMessage messages)
        {
            RuleFor(x => x.Content)
                .NotNull()
                .WithMessage(messages.MsgContentRequired);

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage(messages.MsgContentRequired);

            RuleFor(x => x.Content)
                .MaximumLength(2500)
                .WithMessage(messages.MsgContentMaxLength);

            RuleFor(x => x.MediaFile)
                .MaximumLength(2048)
                .WithMessage(messages.MsgMediaFileTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.MediaFile));

            RuleFor(x => x.MediaType)
                .IsInEnum()
                .WithMessage(messages.MsgInvalidMediaType);
        }
    }
}
