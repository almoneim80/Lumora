namespace Lumora.Services.Core.Messages
{
    public class CourseLessonMessages(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgLessonNotFound => _localization.GetLocalizedString("LessonNotFound");
        public string MsgCourseNotFound => _localization.GetLocalizedString("CourseNotFound");
        public string MsgLessonCreated => _localization.GetLocalizedString("LessonCreated");
        public string MsgLessonUpdated => _localization.GetLocalizedString("LessonUpdated");
        public string MsgLessonDeleted => _localization.GetLocalizedString("LessonDeleted");
        public string MsgLessonRetrieved => _localization.GetLocalizedString("LessonRetrieved");
        public string MsgLessonMustHaveExercise => _localization.GetLocalizedString("LessonMustHaveExercise");
        public string MsgLessonMustHaveAttachment => _localization.GetLocalizedString("LessonMustHaveAttachment");
        public string MsgAtLeastTwoChoices => _localization.GetLocalizedString("AtLeastTwoChoices");
        public string MsgLessonNameRequired => _localization.GetLocalizedString("LessonNameRequired");
        public string MsgLessonFileUrlRequired => _localization.GetLocalizedString("LessonFileUrlRequired");
        public string MsgLessonOrderRequired => _localization.GetLocalizedString("LessonOrderRequired");
        public string MsgLessonOrderInvalid => _localization.GetLocalizedString("LessonOrderInvalid");
        public string MsgLessonDurationRequired => _localization.GetLocalizedString("LessonDurationRequired");
        public string MsgLessonDurationInvalid => _localization.GetLocalizedString("LessonDurationInvalid");
        public string MsgLessonDescriptionRequired => _localization.GetLocalizedString("LessonDescriptionRequired");
        public string MsgCourseIdInvalid => _localization.GetLocalizedString("CourseIdInvalid");
        public string MsgLessonFileUrlInvalid => _localization.GetLocalizedString("LessonFileUrlInvalid");
        public string MsgLessonDescriptionInvalid => _localization.GetLocalizedString("LessonDescriptionInvalid");
        public string MsgLessonCourseIdInvalid => _localization.GetLocalizedString("LessonCourseIdInvalid");
        public string MsgLessonIdRequired => _localization.GetLocalizedString("LessonIdRequired");
        public string MsgLessonOrderIndexInvalid => _localization.GetLocalizedString("LessonOrderIndexInvalid");
        public string MsgLessonNameTooLong => _localization.GetLocalizedString("LessonNameTooLong");
        public string MsgLessonDescriptionTooLong => _localization.GetLocalizedString("LessonDescriptionTooLong");
        public string MsgDtoNull => _localization.GetLocalizedString("DtoNull");
        public string MsgAttachmentCountMismatch => _localization.GetLocalizedString("AttachmentCountMismatch");
        public string MsgInvalidJsonFormat => _localization.GetLocalizedString("InvalidJsonFormat");
    }
}
