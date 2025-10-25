using Lumora.DTOs.Job;

namespace Lumora.Web.Validations.JobVal
{
    public class JobCreateValidator : AbstractValidator<JobCreateDto>
    {
        public JobCreateValidator(JobMessages messages)
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(messages.MsgTitleRequired);

            RuleFor(x => x.Title)
                .MustContainOnlyLettersAndSpaces()
                .WithMessage(messages.MsgTitleInvalid);

            RuleFor(x => x.Title)
                .MustHaveLengthInRange(2, 250);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(messages.MsgDescriptionRequired);

            RuleFor(x => x.Description)
                .MustHaveLengthInRange(10, 2000);

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage(messages.MsgLocationRequired);

            RuleFor(x => x.Location)
                .MustHaveLengthInRange(2, 250);

            RuleFor(x => x.JobType)
                .NotNull().WithMessage(messages.MsgJobTypeRequired);

            RuleFor(x => x.JobType)
               .IsInEnum().WithMessage(messages.MsgInvalidJobType);

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0).WithMessage(messages.MsgSalaryNonNegative);

            RuleFor(x => x.Employer)
                .NotEmpty().WithMessage(messages.MsgEmployerRequired);

            RuleFor(x => x.Employer)
                .MustHaveLengthInRange(2, 250);

            RuleFor(x => x.EmployerInfo)
                .NotEmpty().WithMessage(messages.MsgEmployerInfoRequired);

            RuleFor(x => x.EmployerInfo)
                .MustHaveLengthInRange(2, 1000);

            RuleFor(x => x.WorkplaceCategory)
                .NotNull().WithMessage(messages.MsgWorkplaceCategoryRequired);

            RuleFor(x => x.WorkplaceCategory)
                .IsInEnum().WithMessage(messages.MsgInvalidWorkplaceCategory);

            RuleFor(x => x.ExpiryDate)
                .MustNotBeDefault();

            RuleFor(x => x.ExpiryDate)
                .MustBeValidDate(maxDate: DateTimeOffset.UtcNow, mustBeFuture: true)
                .WithMessage(messages.MsgExpiryDateInvalid);
        }
    }
}
