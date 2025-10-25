namespace Lumora.Web.Validations.ProgramVal
{
    public class TrainingProgramUpdateValidator : AbstractValidator<TrainingProgramUpdateDto>
    {
        public TrainingProgramUpdateValidator(TrainingProgramMessage messages)
        {
            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .MaximumLength(250).WithMessage(messages.MsgProgramNameTooLong);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(2000).WithMessage(messages.MsgProgramDescriptionTooLong);
            });

            When(x => x.Price.HasValue, () =>
            {
                RuleFor(x => x.Price!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage(messages.MsgProgramPriceInvalid);
            });

            When(x => x.Discount.HasValue, () =>
            {
                RuleFor(x => x.Discount!.Value)
                    .GreaterThanOrEqualTo(0).WithMessage(messages.MsgProgramDiscountInvalid);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Logo), () =>
            {
                RuleFor(x => x.Logo)
                    .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    .WithMessage(messages.MsgProgramLogoInvalid);
            });

            When(x => x.HasCertificateExpiration == true, () =>
            {
                RuleFor(x => x.CertificateValidityInMonth)
                    .NotNull().WithMessage(messages.MsgProgramCertificateValidityInvalid);

                RuleFor(x => x.CertificateValidityInMonth!.Value)
                    .GreaterThan(0).WithMessage(messages.MsgProgramCertificateValidityInvalid);
            });
        }
    }
}
