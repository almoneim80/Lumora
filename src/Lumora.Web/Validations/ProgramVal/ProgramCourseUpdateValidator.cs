namespace Lumora.Web.Validations.ProgramVal
{
    public class ProgramCourseUpdateValidator : AbstractValidator<ProgramCourseUpdateDto>
    {
        public ProgramCourseUpdateValidator(CourseMessage messages)
        {
            When(x => x.Id.HasValue, () =>
            {
                RuleFor(x => x.Id!.Value)
                    .GreaterThan(0)
                    .WithMessage(messages.MsgLessonIdInvalid);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage(messages.MsgCourseNameRequired)
                    .MaximumLength(250).WithMessage(messages.MsgCourseNameTooLong);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .NotEmpty().WithMessage(messages.MsgCourseDescriptionRequired)
                    .MaximumLength(2000).WithMessage(messages.MsgCourseDescriptionTooLong);
            });

            When(x => x.Order.HasValue, () =>
            {
                RuleFor(x => x.Order!.Value)
                    .GreaterThan(0)
                    .WithMessage(messages.MsgCourseOrderInvalid);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Logo), () =>
            {
                RuleFor(x => x.Logo)
                    .NotEmpty().WithMessage(messages.MsgCourseLogoRequired)
                    .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    .WithMessage(messages.MsgCourseLogoInvalid);
            });
        }
    }
}
