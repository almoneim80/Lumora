namespace Lumora.Services.Core.Messages
{
    public class PodcastEpisodeMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgDtoNull => _localization.GetLocalizedString("DtoNull");
        public string MsgRequiredFieldsMissing => _localization.GetLocalizedString("RequiredFieldsMissing");
        public string MsgPodcastCreated => _localization.GetLocalizedString("PodcastCreated");
        public string MsgPodcastUpdated => _localization.GetLocalizedString("PodcastUpdated");
        public string MsgEpisodeNotFound => _localization.GetLocalizedString("EpisodeNotFound");
        public string MsgPodcastDeleted => _localization.GetLocalizedString("PodcastDeleted");
        public string MsgPodcastFetched => _localization.GetLocalizedString("PodcastFetched");
        public string MsgPodcastListFetched => _localization.GetLocalizedString("PodcastListFetched");
        public string MsgPodcastTitleRequired => _localization.GetLocalizedString("PodcastTitleRequired");
        public string MsgPodcastTitleTooLong => _localization.GetLocalizedString("PodcastTitleTooLong");
        public string MsgPodcastEpisodeNumberInvalid => _localization.GetLocalizedString("PodcastEpisodeNumberInvalid");
        public string MsgYoutubeUrlRequired => _localization.GetLocalizedString("YoutubeUrlRequired");
        public string MsgYoutubeUrlInvalid => _localization.GetLocalizedString("YoutubeUrlInvalid");
        public string MsgThumbnailUrlRequired => _localization.GetLocalizedString("ThumbnailUrlRequired");
        public string MsgThumbnailUrlInvalid => _localization.GetLocalizedString("ThumbnailUrlInvalid");
        public string MsgFileRequired => _localization.GetLocalizedString("FileRequired");
    }
}
