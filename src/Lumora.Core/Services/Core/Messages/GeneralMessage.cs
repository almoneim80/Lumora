namespace Lumora.Services.Core.Messages
{
    public class GeneralMessage(ILocalizationManager localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES

        public string MsgWelcome => _localization.GetLocalizedString("Welcome");
        public string MsgNullOrEmpty => _localization.GetLocalizedString("NullOrEmpty");
        public string MsgIdInvalid => _localization.GetLocalizedString("IdIsNull");
        public string MsgDataNotFound => _localization.GetLocalizedString("DataNotFound");
        public string MsgAccessDenied => _localization.GetLocalizedString("AccessDenied");
        public string MsgUserNotFound => _localization.GetLocalizedString("UserNotFound");
        public string MsgDataRetrieved => _localization.GetLocalizedString("DataRetrieved");
        public string MsgFailedToDeleted => _localization.GetLocalizedString("FailedToDeleted");
        public string MsgDataDeletedSuccessfully => _localization.GetLocalizedString("DataDeletedSuccessfully");
        public string GetUnexpectedErrorMessage(string value) => _localization.GetLocalizedString("UnExpectedError", value);
        public string MsgPageNumberMin => _localization.GetLocalizedString("PageNumberMin");
        public string MsgPageSizeRange => _localization.GetLocalizedString("PageSizeRange");
    }
}
