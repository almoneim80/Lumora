namespace Lumora.Web.Validations.ProgramVal
{
    public class TrainingProgramCreateValidator : AbstractValidator<TrainingProgramCreateDto>
    {
        public TrainingProgramCreateValidator(TrainingProgramMessage messages)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(messages.MsgProgramNameRequired);

            RuleFor(x => x.Name)
                .MaximumLength(250).WithMessage(messages.MsgProgramNameTooLong);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(messages.MsgProgramDescriptionRequired);

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage(messages.MsgProgramDescriptionTooLong);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage(messages.MsgProgramPriceInvalid);

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage(messages.MsgProgramDiscountInvalid);

            RuleFor(x => x.Logo)
                .NotEmpty().WithMessage(messages.MsgProgramLogoRequired);

            RuleFor(x => x.Logo)
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage(messages.MsgProgramLogoInvalid);

            RuleFor(x => x.CertificateValidityInMonth)
                .GreaterThan(0).When(x => x.HasCertificateExpiration)
                .WithMessage(messages.MsgProgramCertificateValidityInvalid);

            RuleFor(x => x.Audience)
                .NotNull().WithMessage(messages.MsgProgramAudienceRequired)
                .Must(list => list.Any()).WithMessage(messages.MsgProgramAudienceRequired);

            RuleFor(x => x.Requirements)
                .NotNull().WithMessage(messages.MsgProgramRequirementsRequired)
                .Must(list => list.Any()).WithMessage(messages.MsgProgramRequirementsRequired);

            RuleFor(x => x.Topics)
                .NotNull().WithMessage(messages.MsgProgramTopicsRequired)
                .Must(list => list.Any()).WithMessage(messages.MsgProgramTopicsRequired);

            RuleFor(x => x.Goals)
                .NotNull().WithMessage(messages.MsgProgramGoalsRequired)
                .Must(list => list.Any()).WithMessage(messages.MsgProgramGoalsRequired);

            RuleFor(x => x.Outcomes)
                .NotNull().WithMessage(messages.MsgProgramOutcomesRequired)
                .Must(list => list.Any()).WithMessage(messages.MsgProgramOutcomesRequired);

            RuleFor(x => x.Trainers)
                .NotNull().WithMessage(messages.MsgProgramTrainersRequired)
                .Must(list => list.Any()).WithMessage(messages.MsgProgramTrainersRequired);

            //RuleFor(x => x.ProgramCourses)
            //    .NotNull().WithMessage(messages.MsgProgramCoursesRequired)
            //    .Must(list => list.Any()).WithMessage(messages.MsgProgramCoursesRequired);
        }
    }
}
