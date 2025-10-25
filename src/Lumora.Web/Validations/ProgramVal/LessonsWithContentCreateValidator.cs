using Lumora.Web.Validations.TestVal;

namespace Lumora.Web.Validations.ProgramVal
{
    public class LessonsWithContentCreateValidator : AbstractValidator<LessonsWithContentCreateDto>
    {
        public LessonsWithContentCreateValidator(CourseLessonMessages courseMessages)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(courseMessages.MsgLessonNameRequired)
                .MaximumLength(250).WithMessage(courseMessages.MsgLessonNameTooLong);

            RuleFor(x => x.FileUrl)
                .NotEmpty().WithMessage(courseMessages.MsgLessonFileUrlRequired)
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage(courseMessages.MsgLessonFileUrlInvalid);

            RuleFor(x => x.Order)
                .GreaterThan(0).WithMessage(courseMessages.MsgLessonOrderInvalid);

            RuleFor(x => x.DurationInMinutes)
                .GreaterThan(0).WithMessage(courseMessages.MsgLessonDurationInvalid);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(courseMessages.MsgLessonDescriptionRequired)
                .MaximumLength(2000).WithMessage(courseMessages.MsgLessonDescriptionTooLong);
        }
    }
}
