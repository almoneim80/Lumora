namespace Lumora.Web.Validations.ProgramVal
{
    public class CourseLessonAttachmentCreateValidator : AbstractValidator<CourseLessonAttachmenCreatetDto>
    {
        public CourseLessonAttachmentCreateValidator(LessonAttachmentMessage messages)
        {
            RuleFor(x => x.FileUrl)
                .NotEmpty()
                .WithMessage(messages.MsgAttachmentFileUrlRequired);
        }
    }
}
