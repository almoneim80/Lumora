namespace Lumora.Web.Validations.ProgramVal
{
    public class SingleLessonAttachmentCreateValidator : AbstractValidator<SingleLessonAttachmentCreateDto>
    {
        public SingleLessonAttachmentCreateValidator(LessonAttachmentMessage messages)
        {
            RuleFor(x => x.LessonId)
                .GreaterThan(0)
                .WithMessage(messages.MsgLessonIdRequired);

            RuleFor(x => x.FileUrl)
                .NotEmpty()
                .WithMessage(messages.MsgAttachmentFileUrlRequired)
                .Must(url => !string.IsNullOrWhiteSpace(url))
                .WithMessage(messages.MsgAttachmentFileUrlRequired);
        }
    }
}
