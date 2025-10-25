namespace Lumora.Web.Validations.ProgramVal
{
    public class LessonAttachmentUpdateValidator : AbstractValidator<LessonAttachmentUpdateDto>
    {
        public LessonAttachmentUpdateValidator(LessonAttachmentMessage messages)
        {
            When(x => x.LessonId is > 0, () =>
            {
                RuleFor(x => x.LessonId)
                    .NotNull()
                    .GreaterThan(0)
                    .WithMessage(messages.MsgLessonIdRequired);
            });

            When(x => !string.IsNullOrWhiteSpace(x.FileUrl), () =>
            {
                RuleFor(x => x.FileUrl)
                    .NotEmpty()
                    .WithMessage(messages.MsgAttachmentFileUrlRequired);
            });
        }
    }
}
