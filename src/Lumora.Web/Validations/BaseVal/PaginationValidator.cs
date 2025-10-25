namespace Lumora.Web.Validations.BaseVal
{
    public class PaginationValidator : AbstractValidator<PaginationRequestDto>
    {
        public PaginationValidator(GeneralMessage messages)
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(messages.MsgPageNumberMin);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage(messages.MsgPageSizeRange);
        }
    }
}
