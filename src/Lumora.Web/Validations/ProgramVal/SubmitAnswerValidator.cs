namespace Lumora.Web.Validations.ProgramVal
{
    public class SubmitAnswerValidator : AbstractValidator<SubmitAnswerDto>
    {
        public SubmitAnswerValidator(TestMessage messages)
        {
            RuleFor(x => x.ExerciseId)
                .GreaterThan(0)
                .WithMessage(messages.MsgExerciseIdRequired);

            RuleFor(x => x.SelectedChoiceId)
                .GreaterThan(0)
                .WithMessage(messages.MsgSelectedChoiceRequired);
        }
    }
}
