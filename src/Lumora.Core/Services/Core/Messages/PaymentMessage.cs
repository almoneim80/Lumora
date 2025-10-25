namespace Lumora.Services.Core.Messages
{
    public class PaymentMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES

        public string MsgPaymentCreated => _localization.GetLocalizedString("PaymentCreated");
        public string MsgPaymentNotFound => _localization.GetLocalizedString("PaymentNotFound");
        public string MsgPaymentStatusChecked => _localization.GetLocalizedString("PaymentStatusChecked");
        public string MsgPaymentNotCompleted => _localization.GetLocalizedString("PaymentNotCompleted");
        public string MsgPaymentLinkedToItems => _localization.GetLocalizedString("PaymentLinkedToItems");
        public string MsgRefundAmountInvalid => _localization.GetLocalizedString("RefundAmountInvalid");
        public string MsgGatewayRefundFailed => _localization.GetLocalizedString("GatewayRefundFailed");
        public string MsgRefundSuccess => _localization.GetLocalizedString("RefundSuccess");
        public string MsgPaymentDetailsRetrieved => _localization.GetLocalizedString("PaymentDetailsRetrieved");
        public string MsgNoPaymentsFound => _localization.GetLocalizedString("NoPaymentsFound");
        public string MsgPaymentsRetrieved => _localization.GetLocalizedString("PaymentsRetrieved");
        public string MsgOnlyPendingCanBeCancelled => _localization.GetLocalizedString("OnlyPendingCanBeCancelled");
        public string MsgPaymentCancelled => _localization.GetLocalizedString("PaymentCancelled");
        public string MsgPaymentNotFoundForItem => _localization.GetLocalizedString("PaymentNotFoundForItem");
        public string MsgPaymentFoundForItem => _localization.GetLocalizedString("PaymentFoundForItem");
        public string MsgPaymentStarted => _localization.GetLocalizedString("PaymentStarted");
        public string MsgGatewayInitiationFailed => _localization.GetLocalizedString("GatewayInitiationFailed");
    }
}
