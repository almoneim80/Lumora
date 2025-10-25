namespace Lumora.Services.Core.Messages
{
    public class JobMessages(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgMissingTitleOrDescription => _localization.GetLocalizedString("MissingTitleOrDescription");
        public string MsgJobCreatedSuccessfully => _localization.GetLocalizedString("JobCreatedSuccessfully");
        public string MsgJobNotFound => _localization.GetLocalizedString("JobNotFound");
        public string MsgJobUpdatedSuccessfully => _localization.GetLocalizedString("JobUpdatedSuccessfully");
        public string MsgJobDeletedSuccessfully => _localization.GetLocalizedString("JobDeletedSuccessfully");
        public string MsgJobFetchedSuccessfully => _localization.GetLocalizedString("JobFetchedSuccessfully");
        public string MsgJobsFetchedSuccessfully => _localization.GetLocalizedString("JobsFetchedSuccessfully");
        public string MsgJobActivated => _localization.GetLocalizedString("JobActivated");
        public string MsgJobDeactivated => _localization.GetLocalizedString("JobDeactivated");
        public string MsgApplicationSubmitted => _localization.GetLocalizedString("ApplicationSubmitted");
        public string MsgAlreadyApplied => _localization.GetLocalizedString("AlreadyApplied");
        public string MsgApplicationsFetchedSuccessfully => _localization.GetLocalizedString("ApplicationsFetchedSuccessfully");
        public string GetApplicationNotFoundMessage(int id) => _localization.GetLocalizedString("ApplicationNotFound", id.ToString());
        public string GetSameStatusMessage(string status) => _localization.GetLocalizedString("SameStatusAlreadySet", status);
        public string MsgApplicationStatusUpdated => _localization.GetLocalizedString("MsgApplicationStatusUpdated");
        public string MsgNoApplicationsFound => _localization.GetLocalizedString("NoApplicationsFound");
        public string MsgApplicationsRetrieved => _localization.GetLocalizedString("MsgApplicationsRetrieved");
        public string MsgTitleRequired => _localization.GetLocalizedString("TitleRequired");
        public string MsgTitleInvalid => _localization.GetLocalizedString("TitleInvalid");
        public string MsgDescriptionRequired => _localization.GetLocalizedString("DescriptionRequired");
        public string MsgLocationRequired => _localization.GetLocalizedString("LocationRequired");
        public string MsgInvalidJobType => _localization.GetLocalizedString("InvalidJobType");
        public string MsgSalaryNonNegative => _localization.GetLocalizedString("SalaryNonNegative");
        public string MsgEmployerRequired => _localization.GetLocalizedString("EmployerRequired");
        public string MsgEmployerInfoRequired => _localization.GetLocalizedString("EmployerInfoRequired");
        public string MsgInvalidWorkplaceCategory => _localization.GetLocalizedString("InvalidWorkplaceCategory");
        public string MsgExpiryDateFuture => _localization.GetLocalizedString("ExpiryDateFuture");
        public string MsgJobIdRequired => _localization.GetLocalizedString("JobIdRequired");
        public string MsgCoverLetterTooLong => _localization.GetLocalizedString("CoverLetterTooLong");
        public string MsgResumeUrlInvalid => _localization.GetLocalizedString("ResumeUrlInvalid");
        public string MsgJobTitleTooLong => _localization.GetLocalizedString("JobTitleTooLong");
        public string MsgJobDescriptionTooLong => _localization.GetLocalizedString("JobDescriptionTooLong");
        public string MsgJobLocationTooLong => _localization.GetLocalizedString("JobLocationTooLong");
        public string MsgSalaryMustBePositive => _localization.GetLocalizedString("SalaryMustBePositive");
        public string MsgEmployerTooLong => _localization.GetLocalizedString("EmployerTooLong");
        public string MsgEmployerInfoTooLong => _localization.GetLocalizedString("EmployerInfoTooLong");
        public string MsgJobTypeRequired => _localization.GetLocalizedString("JobTypeRequired");
        public string MsgWorkplaceCategoryRequired => _localization.GetLocalizedString("WorkplaceCategoryRequired");
        public string MsgExpiryDateInvalid => _localization.GetLocalizedString("ExpiryDateInvalid");
        public string MsgInvalidJobId => _localization.GetLocalizedString("InvalidJobId");
    }
}
