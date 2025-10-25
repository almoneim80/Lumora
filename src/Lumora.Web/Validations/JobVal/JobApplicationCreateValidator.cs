using Lumora.DTOs.Job;

namespace Lumora.Web.Validations.JobVal
{
    public class JobApplicationCreateValidator : AbstractValidator<JobApplicationCreateDto>
    {
        public JobApplicationCreateValidator(JobMessages messages)
        {
            RuleFor(x => x.JobId)
                .GreaterThan(0)
                .WithMessage(messages.MsgJobIdRequired);

            RuleFor(x => x.CoverLetter)
                .MaximumLength(1000)
                .WithMessage(messages.MsgCoverLetterTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.CoverLetter));

            RuleFor(x => x.ResumeUrl)
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage(messages.MsgResumeUrlInvalid)
                .When(x => !string.IsNullOrWhiteSpace(x.ResumeUrl));
        }
    }
}
