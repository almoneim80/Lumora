namespace Lumora.Services.Core.Messages
{
    public class WheelMessag(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgInvalidId => _localization.GetLocalizedString("InvalidId");
        public string MsgAwardNotFound => _localization.GetLocalizedString("AwardNotFound");
        public string MsgAwardRetrieved => _localization.GetLocalizedString("AwardRetrieved");
        public string MsgWheelAwardsFetched => _localization.GetLocalizedString("WheelAwardsFetched");
        public string MsgDtoNull => _localization.GetLocalizedString("DtoNull");
        public string MsgRequiredFieldsInvalid => _localization.GetLocalizedString("RequiredFieldsInvalid");
        public string MsgWheelAwardCreated => _localization.GetLocalizedString("WheelAwardCreated");
        public string MsgWheelAwardNotFound => _localization.GetLocalizedString("WheelAwardNotFound");
        public string MsgProbabilityInvalid => _localization.GetLocalizedString("ProbabilityInvalid");
        public string MsgWheelAwardUpdated => _localization.GetLocalizedString("WheelAwardUpdated");
        public string MsgWheelAwardDeleted => _localization.GetLocalizedString("WheelAwardDeleted");
    }
}
