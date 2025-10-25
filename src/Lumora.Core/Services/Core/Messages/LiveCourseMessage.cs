namespace Lumora.Services.Core.Messages
{
    public class LiveCourseMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgCourseNotFound => _localization.GetLocalizedString("CourseNotFound");
        public string MsgCourseSubscribersFetchedSuccessfully => _localization.GetLocalizedString("CourseSubscribersFetchedSuccessfully");
        public string MsgMissingCourseFields => _localization.GetLocalizedString("MissingCourseFields");
        public string MsgCourseCreatedSuccessfully => _localization.GetLocalizedString("CourseCreatedSuccessfully");
        public string MsgCourseUpdatedSuccessfully => _localization.GetLocalizedString("CourseUpdatedSuccessfully");
        public string MsgCourseFetchedSuccessfully => _localization.GetLocalizedString("CourseFetchedSuccessfully");
        public string MsgCourseListFetchedSuccessfully => _localization.GetLocalizedString("CourseListFetchedSuccessfully");
        public string MsgCourseHasSubscribers => _localization.GetLocalizedString("CourseHasSubscribers");
        public string MsgCourseDeletedSuccessfully => _localization.GetLocalizedString("CourseDeletedSuccessfully");
        public string MsgCourseAlreadyActive => _localization.GetLocalizedString("CourseAlreadyActive");
        public string MsgCourseAlreadyInactive => _localization.GetLocalizedString("CourseAlreadyInactive");
        public string MsgCourseActivated => _localization.GetLocalizedString("CourseActivated");
        public string MsgCourseDeactivated => _localization.GetLocalizedString("CourseDeactivated");
        public string MsgPaymentItemRequired => _localization.GetLocalizedString("PaymentItemRequired");
        public string MsgInvalidPaymentItem => _localization.GetLocalizedString("InvalidPaymentItem");
        public string MsgAlreadySubscribed => _localization.GetLocalizedString("AlreadySubscribed");
        public string MsgUserSubscribedSuccessfully => _localization.GetLocalizedString("UserSubscribedSuccessfully");
        public string MsgCoursesFetchedSuccessfully => _localization.GetLocalizedString("CoursesFetchedSuccessfully");
        public string MsgLiveCourseTitleRequired => _localization.GetLocalizedString("LiveCourseTitleRequired");
        public string MsgLiveCoursePriceInvalid => _localization.GetLocalizedString("LiveCoursePriceInvalid");
        public string MsgLiveCourseDescriptionRequired => _localization.GetLocalizedString("LiveCourseDescriptionRequired");
        public string MsgLiveCourseImageRequired => _localization.GetLocalizedString("LiveCourseImageRequired");
        public string MsgLiveCourseLessonsRequired => _localization.GetLocalizedString("LiveCourseLessonsRequired");
        public string MsgLessonNameRequired => _localization.GetLocalizedString("LessonNameRequired");
        public string MsgLiveCourseEndDateMustBeFuture => _localization.GetLocalizedString("LiveCourseEndDateMustBeFuture");
        public string MsgLessonFileRequired => _localization.GetLocalizedString("LessonFileRequired");
        public string MsgLessonDurationInvalid => _localization.GetLocalizedString("LessonDurationInvalid");
        public string MsgLessonDescriptionRequired => _localization.GetLocalizedString("LessonDescriptionRequired");
        public string MsgLessonAttachmentFileRequired => _localization.GetLocalizedString("LessonAttachmentFileRequired");
        public string MsgInvalidJsonFormat => _localization.GetLocalizedString("InvalidJsonFormat");
        public string MsgDtoNull => _localization.GetLocalizedString("DtoNull");
        public string MsgLiveCourseStudyWayRequired => _localization.GetLocalizedString("LiveCourseStudyWayRequired");
        public string MsgLiveCourseStartDateRequired => _localization.GetLocalizedString("LiveCourseStartDateRequired");
        public string MsgLiveCourseEndDateRequired => _localization.GetLocalizedString("LiveCourseEndDateRequired");
        public string MsgLiveCourseEndDateMustBeAfterStart => _localization.GetLocalizedString("LiveCourseEndDateMustBeAfterStart");
        public string MsgLiveCourseLinkRequired => _localization.GetLocalizedString("LiveCourseLinkRequired");
        public string MsgLiveCourseLinkInvalid => _localization.GetLocalizedString("LiveCourseLinkInvalid");
        public string MsgLiveCourseLecturerRequired => _localization.GetLocalizedString("LiveCourseLecturerRequired");
        public string MsgLiveCourseStartDateMustBeFuture => _localization.GetLocalizedString("LiveCourseStartDateMustBeFuture");
        public string MsgValidDescriptionRequired => _localization.GetLocalizedString("ValidDescriptionRequired");
        public string MsgLiveCourseTitleLengthRange => _localization.GetLocalizedString("LiveCourseTitleLengthRange");
        public string MsgLiveCourseStudyWayLengthExceeded => _localization.GetLocalizedString("LiveCourseStudyWayLengthExceeded");
        public string MsgLiveCourseLecturerLengthExceeded => _localization.GetLocalizedString("LiveCourseLecturerLengthExceeded");

        public string GetInvalidVideoDurationMessage(int index) =>
            _localization.GetLocalizedString("InvalidVideoDuration").Replace("{index}", (index + 1).ToString());
    }
}
