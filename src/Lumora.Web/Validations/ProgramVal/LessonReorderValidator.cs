namespace Lumora.Web.Validations.ProgramVal
{
    public class LessonReorderValidator : AbstractValidator<LessonReorderDto>
    {
        public LessonReorderValidator(CourseLessonMessages messages)
        {
            RuleFor(x => x.LessonId)
                .GreaterThan(0)
                .WithMessage(messages.MsgLessonIdRequired);

            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage(messages.MsgLessonOrderIndexInvalid);
        }
    }
}
