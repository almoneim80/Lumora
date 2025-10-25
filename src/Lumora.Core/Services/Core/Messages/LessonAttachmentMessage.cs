namespace Lumora.Services.Core.Messages
{
    public class LessonAttachmentMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgLessonNotFound => _localization.GetLocalizedString("LessonNotFound");
        public string MsgAttachmentAdded => _localization.GetLocalizedString("AttachmentAdded");
        public string MsgAttachmentNotFound => _localization.GetLocalizedString("AttachmentNotFound");
        public string MsgAttachmentUpdated => _localization.GetLocalizedString("AttachmentUpdated");
        public string MsgAttachmentDeleted => _localization.GetLocalizedString("AttachmentDeleted");
        public string MsgAttachmentRetrieved => _localization.GetLocalizedString("AttachmentRetrieved");
        public string MsgAttachmentsRetrieved => _localization.GetLocalizedString("AttachmentsRetrieved");
        public string MsgOpenCountIncremented => _localization.GetLocalizedString("OpenCountIncremented");
        public string MsgAttachmentFileUrlRequired => _localization.GetLocalizedString("AttachmentFileUrlRequired");
        public string MsgLessonIdRequired => _localization.GetLocalizedString("LessonIdRequired");
        public string MsgAttachmentIdMustBeGreaterThanZero => _localization.GetLocalizedString("AttachmentIdMustBeGreaterThanZero");
        public string MsgFileRequired => _localization.GetLocalizedString("FileRequired");
    }
}
