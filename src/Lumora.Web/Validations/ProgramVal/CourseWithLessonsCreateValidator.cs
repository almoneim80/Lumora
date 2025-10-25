namespace Lumora.Web.Validations.ProgramVal
{
    public class CourseWithLessonsCreateValidator : AbstractValidator<CourseWithLessonsCreateDto>
    {
        public CourseWithLessonsCreateValidator(TrainingProgramMessage messages)
        {
            RuleFor(x => x.ProgramId)
                .GreaterThan(0)
                .WithMessage(messages.MsgProgramIdRequired);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(messages.MsgCourseNameRequired);

            RuleFor(x => x.Name)
                .MaximumLength(250)
                .WithMessage(messages.MsgCourseNameTooLong);

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage(messages.MsgCourseDescriptionRequired);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage(messages.MsgCourseDescriptionTooLong);

            RuleFor(x => x.Order)
                .GreaterThan(0)
                .WithMessage(messages.MsgCourseOrderInvalid);

            RuleFor(x => x.Logo)
                .NotEmpty()
                .WithMessage(messages.MsgCourseLogoRequired);

            RuleFor(x => x.Logo)
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage(messages.MsgCourseLogoInvalid);
        }
    }
}
