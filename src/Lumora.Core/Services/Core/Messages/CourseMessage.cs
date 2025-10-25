namespace Lumora.Services.Core.Messages
{
    public class CourseMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        //MESSAGES
        public string MsgIncomplete => _localization.GetLocalizedString("InCompleteProgram");
        public string MsgProgramNotFound => _localization.GetLocalizedString("ProgramNotFound");
        public string MsgNoLessons => _localization.GetLocalizedString("NoLessons");
        public string MsgCourseCreated => _localization.GetLocalizedString("CourseCreated");
        public string MsgCourseNotFound => _localization.GetLocalizedString("CourseNotFound");
        public string MsgCourseUpdated => _localization.GetLocalizedString("CourseUpdated");
        public string MsgCourseDeleted => _localization.GetLocalizedString("CourseDeleted");
        public string MsgTestMustHaveQuestions => _localization.GetLocalizedString("TestMustHaveQuestions");
        public string GetNoLessonAttachment(string value) => _localization.GetLocalizedStringWithReplaced("NoLessonAttachment", value);
        public string GetNoLessonExercise(string value) => _localization.GetLocalizedStringWithReplaced("NoLessonExercise", value);
        public string GetNoExercisechoices(string value) => _localization.GetLocalizedStringWithReplaced("NoExercisechoices", value);
        public string MsgCertificateAlreadyIssued => _localization.GetLocalizedString("CertificateAlreadyIssued");
        public string MsgProgramIdRequired => _localization.GetLocalizedString("ProgramIdRequired");
        public string MsgCourseNameTooLong => _localization.GetLocalizedString("CourseNameTooLong");
        public string MsgCourseDescriptionTooLong => _localization.GetLocalizedString("CourseDescriptionTooLong");
        public string MsgCourseOrderInvalid => _localization.GetLocalizedString("CourseOrderInvalid");
        public string MsgCourseLogoInvalid => _localization.GetLocalizedString("CourseLogoInvalid");
        public string MsgCourseIdInvalid => _localization.GetLocalizedString("CourseIdInvalid");
        public string MsgCourseNameRequired => _localization.GetLocalizedString("CourseNameRequired");
        public string MsgCourseDescriptionRequired => _localization.GetLocalizedString("CourseDescriptionRequired");
        public string MsgCourseLogoRequired => _localization.GetLocalizedString("CourseLogoRequired");
        public string MsgLessonIdInvalid => _localization.GetLocalizedString("LessonIdInvalid");
        public string MsgDtoNull => _localization.GetLocalizedString("DtoNull");
        public string MsgInvalidJsonFormat => _localization.GetLocalizedString("InvalidJsonFormat");
        public string GetInvalidVideoDurationMessage(int index) =>
            _localization.GetLocalizedString("InvalidVideoDuration").Replace("{index}", (index + 1).ToString());
    }
}
