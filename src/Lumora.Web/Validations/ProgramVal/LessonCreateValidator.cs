namespace Lumora.Web.Validations.ProgramVal
{
    public class LessonCreateValidator : AbstractValidator<LessonCreateDto>
    {
        public LessonCreateValidator(CourseLessonMessages messages)
        {
            RuleFor(x => x.Name)
                .MustNotBeDefault()
                .WithMessage(messages.MsgLessonNameRequired);

            RuleFor(x => x.FileUrl)
                .MustNotBeDefault()
                .MustBeValidAttachment()
                .WithMessage(messages.MsgLessonFileUrlInvalid);

            RuleFor(x => x.Order)
                .GreaterThan(0)
                .WithMessage(messages.MsgLessonOrderInvalid);

            RuleFor(x => x.DurationInMinutes)
                .GreaterThan(0)
                .WithMessage(messages.MsgLessonDurationInvalid);

            RuleFor(x => x.Description)
                .MustNotBeDefault()
                .MustBeValidDescription()
                .WithMessage(messages.MsgLessonDescriptionInvalid);
        }
    }
}
