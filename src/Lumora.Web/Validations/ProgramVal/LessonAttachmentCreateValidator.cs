namespace Lumora.Web.Validations.ProgramVal
{
    public class LessonAttachmentCreateValidator : AbstractValidator<LessonAttachmentCreateDto>
    {
        public LessonAttachmentCreateValidator(LessonAttachmentMessage messages)
        {
            RuleFor(x => x.FileUrl)
                .NotEmpty()
                .WithMessage(messages.MsgAttachmentFileUrlRequired)
                .Must(url => !string.IsNullOrWhiteSpace(url))
                .WithMessage(messages.MsgAttachmentFileUrlRequired);
        }
    }
}
