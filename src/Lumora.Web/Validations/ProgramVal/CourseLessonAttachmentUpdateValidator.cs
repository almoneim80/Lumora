namespace Lumora.Web.Validations.ProgramVal
{
    public class CourseLessonAttachmentUpdateValidator : AbstractValidator<CourseLessonAttachmentUpdateDto>
    {
        public CourseLessonAttachmentUpdateValidator(LessonAttachmentMessage messages)
        {
            When(x => x.Id.HasValue, () =>
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0)
                    .WithMessage(messages.MsgAttachmentIdMustBeGreaterThanZero);
            });

            When(x => string.IsNullOrWhiteSpace(x.FileUrl) == false, () =>
            {
                RuleFor(x => x.FileUrl)
                    .NotEmpty()
                    .WithMessage(messages.MsgAttachmentFileUrlRequired);
            });
        }
    }
}
