namespace Lumora.Web.Validations.ProgramVal
{
    public class CourseUpdateValidator : AbstractValidator<CourseUpdateDto>
    {
        public CourseUpdateValidator(CourseMessage messages)
        {
            When(x => x.ProgramId.HasValue, () =>
            {
                RuleFor(x => x.ProgramId!.Value)
                    .GreaterThan(0)
                    .WithMessage(messages.MsgProgramIdRequired);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name!)
                    .MaximumLength(250)
                    .WithMessage(messages.MsgCourseNameTooLong);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
            {
                RuleFor(x => x.Description!)
                    .MaximumLength(2000)
                    .WithMessage(messages.MsgCourseDescriptionTooLong);
            });

            When(x => x.Order.HasValue, () =>
            {
                RuleFor(x => x.Order!.Value)
                    .GreaterThan(0)
                    .WithMessage(messages.MsgCourseOrderInvalid);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Logo), () =>
            {
                RuleFor(x => x.Logo!)
                    .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    .WithMessage(messages.MsgCourseLogoInvalid);
            });
        }
    }
}
