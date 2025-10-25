namespace Lumora.Services.Core.Messages
{
    public class ProgressMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgLessonAlreadyCompleted => _localization.GetLocalizedString("LessonAlreadyCompleted");
        public string MsgLessonCompleted => _localization.GetLocalizedString("LessonCompleted");
        public string MsgLessonNotCompleted => _localization.GetLocalizedString("LessonNotCompleted");
        public string MsgLessonProgressRetrieved => _localization.GetLocalizedString("LessonProgressRetrieved");
        public string MsgNoCompletedLessons => _localization.GetLocalizedString("NoCompletedLessons");
        public string MsgCompletedLessonsRetrieved => _localization.GetLocalizedString("CompletedLessonsRetrieved");
        public string MsgCourseNotFound => _localization.GetLocalizedString("CourseNotFound");
        public string MsgCourseProgressUpdated => _localization.GetLocalizedString("CourseProgressUpdated");
        public string MsgProgressNotFound => _localization.GetLocalizedString("ProgressNotFound");
        public string MsgCourseProgressRetrieved => _localization.GetLocalizedString("CourseProgressRetrieved");
        public string MsgNoCourseProgressFound => _localization.GetLocalizedString("NoCourseProgress");
        public string MsgNoProgramCoursesFound => _localization.GetLocalizedString("NoProgramCourses");
        public string MsgProgramProgressUpdated => _localization.GetLocalizedString("ProgramProgressUpdated");
        public string MsgProgramProgressNotFound => _localization.GetLocalizedString("ProgramProgressNotFound");
        public string MsgProgramProgressRetrieved => _localization.GetLocalizedString("ProgramProgressRetrieved");
        public string MsgLessonNotFound => _localization.GetLocalizedString("LessonNotFound");
        public string MsgSessionAlreadyActive => _localization.GetLocalizedString("SessionAlreadyActive");
        public string MsgSessionStarted => _localization.GetLocalizedString("SessionStarted");
        public string MsgNoActiveSession => _localization.GetLocalizedString("NoActiveSession");
        public string MsgSessionEnded => _localization.GetLocalizedString("SessionEnded");
        public string MsgProgramNotFound => _localization.GetLocalizedString("ProgramNotFound");
        public string MsgNoEnrolledUser => _localization.GetLocalizedString("NoEnrolledUser");
        public string MsgProgressSynced => _localization.GetLocalizedString("ProgressSynced");
        public string MsgNoLessonsFoundForcourse => _localization.GetLocalizedString("NoLessonsFoundForcourse");
        public string MsgLessonMarkedAsCompletedNow => _localization.GetLocalizedString("LessonMarkedAsCompletedNow");
        public string MsgUserIdRequired => _localization.GetLocalizedString("UserIdRequired");
        public string MsgCourseOrProgramRequired => _localization.GetLocalizedString("CourseOrProgramRequired");
    }
}
