namespace Lumora.Services.Core.Messages
{
    public class CertificateMessages(ILocalizationManager localization) : CourseMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgCertificateIssued => _localization.GetLocalizedString("CertificateIssued");
        public string MsgCertificateCount => _localization.GetLocalizedString("CertificateCountRetrieved");
        public string MsgNoCertificatesFound => _localization.GetLocalizedString("NoCertificatesFound");
        public string MsgCertificateCountRetrieved => _localization.GetLocalizedString("CertificateCountRetrieved");
        public string MsgCertificateListRetrieved => _localization.GetLocalizedString("CertificateListRetrieved");
        public string MsgRevocationReasonRequired => _localization.GetLocalizedString("RevocationReasonRequired");
        public string MsgCertificateCannotBeRevoked => _localization.GetLocalizedString("CertificateCannotBeRevoked");
        public string MsgCertificateRevoked => _localization.GetLocalizedString("CertificateRevoked");
        public string MsgCertificateCannotGenerateCode => _localization.GetLocalizedString("CertificateCannotGenerateCode");
        public string MsgVerificationCodeGenerated => _localization.GetLocalizedString("VerificationCodeGenerated");
        public string MsgVerificationCodeRetrieved => _localization.GetLocalizedString("VerificationCodeRetrieved");
        public string MsgCertificateNotIssued => _localization.GetLocalizedString("CertificateNotIssued");
        public string MsgCertificatePdfGenerated => _localization.GetLocalizedString("CertificatePdfGenerated");
        public string MsgCertificateVerified => _localization.GetLocalizedString("CertificateVerified");
        public string MsgVerificationCodeRequired => _localization.GetLocalizedString("VerificationCodeRequired");
        public string MsgVerificationCodeInvalid => _localization.GetLocalizedString("VerificationCodeInvalid");
        public string MsgCertificateExpired => _localization.GetLocalizedString("CertificateExpired");
    }
}
