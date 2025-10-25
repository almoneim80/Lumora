namespace Lumora.Services.Core.Messages
{
    public class AffiliateMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgPromoCodeExists => _localization.GetLocalizedString("PromoCodeExists");
        public string MsgPromoCodeCreatedSuccess => _localization.GetLocalizedString("PromoCodeCreatedSuccess");
        public string MsgTrainingProgramNotFound => _localization.GetLocalizedString("TrainingProgramNotFound");
        public string MsgUserIdRequired => _localization.GetLocalizedString("UserIdRequired");
        public string MsgDtoNull => _localization.GetLocalizedString("DtoNull");
        public string MsgPromoCodeUsageRegisteredSuccess => _localization.GetLocalizedString("PromoCodeUsageRegisteredSuccess");
        public string MsgPaymentIdRequired => _localization.GetLocalizedString("PaymentIdRequired");
        public string MsgPaymentNotFound => _localization.GetLocalizedString("PaymentNotFound");
        public string MsgPromoCodeMissingFromPayment => _localization.GetLocalizedString("PromoCodeMissingFromPayment");
        public string MsgPromoCodeInactive => _localization.GetLocalizedString("PromoCodeInactive");
        public string MsgPromoCodeUsageAlreadyExists => _localization.GetLocalizedString("PromoCodeUsageAlreadyExists");
        public string MsgNoActivePromoCodes => _localization.GetLocalizedString("NoActivePromoCodes");
        public string MsgAllPromoCodesDeactivated => _localization.GetLocalizedString("AllPromoCodesDeactivated");
        public string MsgNoPromoCodesFound => _localization.GetLocalizedString("NoPromoCodesFound");
        public string MsgPromoCodeReportSuccess => _localization.GetLocalizedString("PromoCodeReportSuccess");
        public string MsgPromoCodeIdRequired => _localization.GetLocalizedString("PromoCodeIdRequired");
        public string MsgPromoCodeNotFound => _localization.GetLocalizedString("PromoCodeNotFound");
        public string MsgPromoCodeAlreadyActive => _localization.GetLocalizedString("PromoCodeAlreadyActive");
        public string MsgPromoCodeReactivated => _localization.GetLocalizedString("PromoCodeReactivated");
    }
}
