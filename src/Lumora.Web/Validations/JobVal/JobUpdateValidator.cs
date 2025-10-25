using Lumora.DTOs.Job;

namespace Lumora.Web.Validations.JobVal
{
    public class JobUpdateValidator : AbstractValidator<JobUpdateDto>
    {
        public JobUpdateValidator(JobMessages messages)
        {
            RuleFor(x => x.Title)
                .MaximumLength(250)
                .WithMessage(messages.MsgJobTitleTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(5000)
                .WithMessage(messages.MsgJobDescriptionTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Location)
                .MaximumLength(250)
                .WithMessage(messages.MsgJobLocationTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.Location));

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0)
                .WithMessage(messages.MsgSalaryMustBePositive)
                .When(x => x.Salary.HasValue);

            RuleFor(x => x.Employer)
                .MaximumLength(250)
                .WithMessage(messages.MsgEmployerTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.Employer));

            RuleFor(x => x.EmployerInfo)
                .MaximumLength(1000)
                .WithMessage(messages.MsgEmployerInfoTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.EmployerInfo));
        }
    }
}
