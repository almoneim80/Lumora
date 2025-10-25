namespace Lumora.Services.Core.Messages
{
    public class TrainingProgramMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGE
        public string MsgProgramCreated => _localization.GetLocalizedString("ProgramCreated");
        public string MsgProgramNotFound => _localization.GetLocalizedString("ProgramNotFound");
        public string MsgProgramsRetrieved => _localization.GetLocalizedString("ProgramsRetrieved");
        public string MsgProgramRetrieved => _localization.GetLocalizedString("ProgramRetrieved");
        public string MsgProgramDeleted => _localization.GetLocalizedString("ProgramDeleted");
        public string MsgProgramUpdated => _localization.GetLocalizedString("ProgramUpdated");
        public string MsgCannotDeleteProgram => _localization.GetLocalizedString("CannotDeleteProgram");
        public string MsgCompletionReportRetrieved => _localization.GetLocalizedString("CompletionReportRetrieved");
        public string MsgCompletionReportNotFound => _localization.GetLocalizedString("CompletionReportNotFound");
        public string MsgCourseNotFound => _localization.GetLocalizedString("CourseNotFound");
        public string MsgCoursesRetrieved => _localization.GetLocalizedString("CoursesRetrieved");
        public string MsgProgramIdRequired => _localization.GetLocalizedString("ProgramIdRequired");
        public string MsgCourseNameRequired => _localization.GetLocalizedString("CourseNameRequired");
        public string MsgCourseNameTooLong => _localization.GetLocalizedString("CourseNameTooLong");
        public string MsgCourseDescriptionRequired => _localization.GetLocalizedString("CourseDescriptionRequired");
        public string MsgCourseDescriptionTooLong => _localization.GetLocalizedString("CourseDescriptionTooLong");
        public string MsgCourseOrderInvalid => _localization.GetLocalizedString("CourseOrderInvalid");
        public string MsgCourseLogoRequired => _localization.GetLocalizedString("CourseLogoRequired");
        public string MsgCourseLogoInvalid => _localization.GetLocalizedString("CourseLogoInvalid");
        public string MsgLessonsListRequired => _localization.GetLocalizedString("LessonsListRequired");
        public string MsgProgramNameRequired => _localization.GetLocalizedString("ProgramNameRequired");
        public string MsgProgramNameTooLong => _localization.GetLocalizedString("ProgramNameTooLong");
        public string MsgProgramDescriptionRequired => _localization.GetLocalizedString("ProgramDescriptionRequired");
        public string MsgProgramDescriptionTooLong => _localization.GetLocalizedString("ProgramDescriptionTooLong");
        public string MsgProgramPriceInvalid => _localization.GetLocalizedString("ProgramPriceInvalid");
        public string MsgProgramDiscountInvalid => _localization.GetLocalizedString("ProgramDiscountInvalid");
        public string MsgProgramLogoRequired => _localization.GetLocalizedString("ProgramLogoRequired");
        public string MsgProgramLogoInvalid => _localization.GetLocalizedString("ProgramLogoInvalid");
        public string MsgProgramAudienceRequired => _localization.GetLocalizedString("ProgramAudienceRequired");
        public string MsgProgramRequirementsRequired => _localization.GetLocalizedString("ProgramRequirementsRequired");
        public string MsgProgramGoalsRequired => _localization.GetLocalizedString("ProgramGoalsRequired");
        public string MsgProgramTopicsRequired => _localization.GetLocalizedString("ProgramTopicsRequired");
        public string MsgProgramOutcomesRequired => _localization.GetLocalizedString("ProgramOutcomesRequired");
        public string MsgProgramTrainersRequired => _localization.GetLocalizedString("ProgramTrainersRequired");
        public string MsgProgramCoursesRequired => _localization.GetLocalizedString("ProgramCoursesRequired");
        public string MsgProgramCertificateValidityRequired => _localization.GetLocalizedString("ProgramCertificateValidityRequired");
        public string MsgProgramCertificateValidityInvalid => _localization.GetLocalizedString("ProgramCertificateValidityInvalid");
        public string MsgProgramNoCourses => _localization.GetLocalizedString("ProgramNoCourses");
        public string MsgProgramNoLessons => _localization.GetLocalizedString("ProgramNoLessons");
        public string MsgProgramNoAttachments => _localization.GetLocalizedString("ProgramNoAttachments");
    }
}
