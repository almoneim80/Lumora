namespace Lumora.Web.Validations.AuthenticationVal
{
    public class CompleteUserDataValidator : AbstractValidator<CompleteUserDataDto>
    {
        public CompleteUserDataValidator(AuthenticationMessage messages)
        {
            RuleFor(x => x.DateOfBirth)
                .MustNotBeDefault();

            RuleFor(x => x.DateOfBirth)
                .MustBeValidDate(maxDate: DateTimeOffset.UtcNow.AddYears(-18), mustBePast: true)
                .WithMessage(messages.MsgDateOfBirthInvalid);

            RuleFor(x => x.AboutMe)
                .MustNotBeDefault()
                .WithMessage(messages.MsgAboutMeRequired);

            RuleFor(x => x.AboutMe)
                .MustBeValidDescription(5, 500, allowSpecialCharacters: false)
                .WithMessage(messages.MsgAboutMeInvalid);

            RuleFor(x => x.Avatar)
                .MustBeValidImage()
                .WithMessage(messages.MsgAvatarInvalid);
        }
    }
}
