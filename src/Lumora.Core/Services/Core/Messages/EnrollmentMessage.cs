namespace Lumora.Services.Core.Messages
{
    public class EnrollmentMessage(ILocalizationManager localization) : CourseMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        public string MsgAlreadyEnrolled => _localization.GetLocalizedString("AlreadyEnrolledInProgram");
        public string MsgEnrolledSuccess => _localization.GetLocalizedString("EnrolledSuccessfuly");
        public string MsgNoEnrolledUsers => _localization.GetLocalizedString("NoEnrolledUser");
        public string MsgEnrolledUserRetrieved => _localization.GetLocalizedString("EnrolledUserRetrieved");
        public string MsgUserNotEnrolled => _localization.GetLocalizedString("UserNotEnrolled");
        public string MsgUserUnEnrolled => _localization.GetLocalizedString("UserUnEnrolled");
        public string MsgEnrollmentNotFound => _localization.GetLocalizedString("EnrollmentNotFound");
        public string MsgEnrollmentStatusUpdated => _localization.GetLocalizedString("EnrollmentStatusUpdated");
    }
}
