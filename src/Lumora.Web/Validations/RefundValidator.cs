using FluentValidation;

namespace Lumora.Validations;

public class RefundCreateValidator : AbstractValidator<RefundCreateDto>
{
    public RefundCreateValidator()
    {
        RuleFor(x => x.PaymentId).MustNotBeDefault();

        RuleFor(x => x.Reason).MustNotBeDefault();
        RuleFor(x => x.Reason).MustHaveLengthInRange(2, 250);
    }
}

public class RefundUpdateValidator : AbstractValidator<RefundUpdateDto>
{
    public RefundUpdateValidator()
    {
        RuleFor(x => x.PaymentId).MustNotBeDefault();

        //RuleFor(x => x.Amount).MustBeInRange(0.01m, decimal.MaxValue);

        //RuleFor(x => x.Reason).MustHaveLengthInRange(2, 250);
    }
}
