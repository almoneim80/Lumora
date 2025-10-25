namespace Lumora.Services.Core.Messages
{
    public class TrackingMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgTrackingCodeRequired => _localization.GetLocalizedString("TrackingCodeRequired");
        public string MsgAffiliateLinkNotFound => _localization.GetLocalizedString("AffiliateLinkNotFound");
        public string MsgClickTrackedSuccess => _localization.GetLocalizedString("ClickTrackedSuccess");
        public string MsgOrderTotalInvalid => _localization.GetLocalizedString("OrderTotalInvalid");
        public string MsgProductNotFound => _localization.GetLocalizedString("ProductNotFound");
        public string MsgOrderTrackedSuccess => _localization.GetLocalizedString("OrderTrackedSuccess");
        public string MsgAffiliateResolved => _localization.GetLocalizedString("AffiliateResolved");
        public string MsgDuplicateOrderAttempt => _localization.GetLocalizedString("DuplicateOrderAttempt");
        public string MsgClickExpiredOrNotFound => _localization.GetLocalizedString("ClickExpiredOrNotFound");
        public string MsgSelfClickDetected => _localization.GetLocalizedString("SelfClickDetected");
        public string MsgTooManyClicksFromSameIp => _localization.GetLocalizedString("TooManyClicksFromSameIp");
        public string MsgClickSessionIdRequired => _localization.GetLocalizedString("ClickSessionIdRequired");
        public string MsgDuplicateClickSession => _localization.GetLocalizedString("DuplicateClickSession");
        public string MsgDuplicateClickSameProductDevice => _localization.GetLocalizedString("DuplicateClickSameProductDevice");
        public string MsgBotUserAgentRejected => _localization.GetLocalizedString("BotUserAgentRejected");
        public string MsgInvalidReferralCode => _localization.GetLocalizedString("InvalidReferralCode");
        public string MsgIpRequired => _localization.GetLocalizedString("IpRequired");
        public string MsgUserAgentRequired => _localization.GetLocalizedString("UserAgentRequired");
        public string MsgCommissionNotFound => _localization.GetLocalizedString("CommissionNotFound");
        public string MsgCommissionAlreadyApproved => _localization.GetLocalizedString("CommissionAlreadyApproved");
        public string MsgCommissionApproved => _localization.GetLocalizedString("CommissionApproved");
    }
}
