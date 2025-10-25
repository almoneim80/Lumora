using Lumora.DTOs.LiveCourse;

namespace Lumora.Web.Validations.LiveCourseValidators
{
    public class LiveCourseUpdateValidator : AbstractValidator<LiveCourseUpdateFormDto>
    {
        public LiveCourseUpdateValidator(LiveCourseMessage messages)
        {
            RuleFor(x => x.Title)
                .MinimumLength(3).MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Title))
                .WithMessage(messages.MsgLiveCourseTitleLengthRange);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue)
                .WithMessage(messages.MsgLiveCoursePriceInvalid);

            RuleFor(x => x.Description)
                .MustBeValidDescription(5, 1000, allowSpecialCharacters: false)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage(messages.MsgValidDescriptionRequired);

            RuleFor(x => x.StudyWay)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.StudyWay))
                .WithMessage(messages.MsgLiveCourseStudyWayLengthExceeded);

            RuleFor(x => x.StartDate)
                .MustBeValidDate(mustBeFuture: true)
                .When(x => x.StartDate.HasValue)
                .WithMessage(messages.MsgLiveCourseStartDateMustBeFuture);

            RuleFor(x => x.EndDate)
                .MustBeValidDate(mustBeFuture: true)
                .When(x => x.EndDate.HasValue)
                .WithMessage(messages.MsgLiveCourseEndDateMustBeFuture);

            RuleFor(x => x.Link)
                .Must(link => Uri.TryCreate(link, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.Link))
                .WithMessage(messages.MsgLiveCourseLinkInvalid);

            RuleFor(x => x.Lecturer)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Lecturer))
                .WithMessage(messages.MsgLiveCourseLecturerLengthExceeded);
        }
    }
}
