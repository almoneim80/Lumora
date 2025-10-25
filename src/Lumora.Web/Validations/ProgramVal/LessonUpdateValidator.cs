namespace Lumora.Web.Validations.ProgramVal
{
    public class LessonUpdateValidator : AbstractValidator<LessonUpdateDto>
    {
        public LessonUpdateValidator(CourseLessonMessages messages)
        {
            When(x => x.CourseId.HasValue, () =>
            {
                RuleFor(x => x.CourseId!.Value)
                    .GreaterThan(0)
                    .WithMessage(messages.MsgLessonCourseIdInvalid);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .MustNotBeDefault()
                    .WithMessage(messages.MsgLessonNameRequired);
            });

            When(x => !string.IsNullOrWhiteSpace(x.FileUrl), () =>
            {
                RuleFor(x => x.FileUrl)
                    .MustBeValidAttachment()
                    .WithMessage(messages.MsgLessonFileUrlInvalid);
            });

            When(x => x.Order.HasValue, () =>
            {
                RuleFor(x => x.Order!.Value)
                    .GreaterThan(0)
                    .WithMessage(messages.MsgLessonOrderInvalid);
            });

            When(x => x.DurationInMinutes.HasValue, () =>
            {
                RuleFor(x => x.DurationInMinutes!.Value)
                    .GreaterThan(0)
                    .WithMessage(messages.MsgLessonDurationInvalid);
            });
        }
    }
}
