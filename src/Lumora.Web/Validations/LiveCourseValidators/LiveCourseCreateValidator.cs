using Lumora.DTOs.LiveCourse;

namespace Lumora.Web.Validations.LiveCourseValidators
{
    public class LiveCourseCreateValidator : AbstractValidator<LiveCourseCreateFormDto>
    {
        public LiveCourseCreateValidator(LiveCourseMessage messages)
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(messages.MsgLiveCourseTitleRequired);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage(messages.MsgLiveCoursePriceInvalid);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(messages.MsgLiveCourseDescriptionRequired);

            RuleFor(x => x.Description)
                .MustBeValidDescription(5, 1000, allowSpecialCharacters: false)
                .WithMessage(messages.MsgValidDescriptionRequired);

            RuleFor(x => x.StudyWay)
                .NotEmpty().WithMessage(messages.MsgLiveCourseDescriptionRequired);

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage(messages.MsgLiveCourseStartDateRequired);

            RuleFor(x => x.StartDate)
            .MustBeValidDate(mustBeFuture: true)
            .WithMessage(messages.MsgLiveCourseStartDateMustBeFuture);

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage(messages.MsgLiveCourseEndDateRequired)
                .GreaterThan(x => x.StartDate).WithMessage(messages.MsgLiveCourseEndDateMustBeAfterStart);

            RuleFor(x => x.Link)
                .NotEmpty().WithMessage(messages.MsgLiveCourseLinkRequired)
                .Must(link => Uri.TryCreate(link, UriKind.Absolute, out _))
                .WithMessage(messages.MsgLiveCourseLinkInvalid);

            RuleFor(x => x.Lecturer)
                .NotEmpty().WithMessage(messages.MsgLiveCourseLecturerRequired);
        }
    }
}
