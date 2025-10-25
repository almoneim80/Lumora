namespace Lumora.Web.Validations.ProgramVal
{
    public class TrainingProgressCreateValidator : AbstractValidator<TrainingProgressCreateDto>
    {
        public TrainingProgressCreateValidator(ProgressMessage messages)
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(messages.MsgUserIdRequired);

            RuleFor(x => x)
                .Must(x => x.CourseId.HasValue || x.ProgramId.HasValue)
                .WithMessage(messages.MsgCourseOrProgramRequired);
        }
    }
}
