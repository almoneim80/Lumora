namespace Lumora.Services.Core.Messages
{
    public class ClubMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgPostDtoNull => _localization.GetLocalizedString("PostDtoNull");
        public string MsgPostContentRequired => _localization.GetLocalizedString("PostContentRequired");
        public string MsgPostCreated => _localization.GetLocalizedString("PostCreated");
        public string MsgPostNotFound => _localization.GetLocalizedString("PostNotFound");
        public string MsgUnauthorizedDelete => _localization.GetLocalizedString("UnauthorizedDelete");
        public string MsgPostDeleted => _localization.GetLocalizedString("PostDeleted");
        public string MsgPostRetrieved => _localization.GetLocalizedString("PostRetrieved");
        public string MsgNoPostsFound => _localization.GetLocalizedString("NoPostsFound");
        public string MsgPostsRetrieved => _localization.GetLocalizedString("PostsRetrieved");
        public string MsgUserIdRequired => _localization.GetLocalizedString("UserIdRequired");
        public string MsgNoUserPostsFound => _localization.GetLocalizedString("NoUserPostsFound");
        public string MsgUserPostsRetrieved => _localization.GetLocalizedString("UserPostsRetrieved");
        public string MsgDtoNull => _localization.GetLocalizedString("DtoNull");
        public string MsgPostIdInvalid => _localization.GetLocalizedString("PostIdInvalid");
        public string MsgAdminIdRequired => _localization.GetLocalizedString("AdminIdRequired");
        public string MsgPostAlreadyReviewed => _localization.GetLocalizedString("PostAlreadyReviewed");
        public string MsgRejectionReasonRequired => _localization.GetLocalizedString("RejectionReasonRequired");
        public string MsgPostReviewed => _localization.GetLocalizedString("PostReviewed");
        public string MsgNoPendingPosts => _localization.GetLocalizedString("NoPendingPosts");
        public string MsgPendingPostsRetrieved => _localization.GetLocalizedString("PendingPostsRetrieved");
        public string MsgPostAlreadyLiked => _localization.GetLocalizedString("PostAlreadyLiked");
        public string MsgPostLikedSuccessfully => _localization.GetLocalizedString("PostLikedSuccessfully");
        public string MsgPostNotYetLiked => _localization.GetLocalizedString("PostNotYetLiked");
        public string MsgPostUnlikedSuccessfully => _localization.GetLocalizedString("PostUnlikedSuccessfully");
        public string MsgLikeStatusRetrieved => _localization.GetLocalizedString("LikeStatusRetrieved");
        public string MsgPostLikeCountRetrieved => _localization.GetLocalizedString("PostLikeCountRetrieved");
        public string MsgAmbassadorAssigned => _localization.GetLocalizedString("AmbassadorAssigned");
        public string MsgAmbassadorInvalidDates => _localization.GetLocalizedString("AmbassadorInvalidDates");
        public string MsgAmbassadorIdInvalid => _localization.GetLocalizedString("AmbassadorIdInvalid");
        public string MsgAmbassadorNotFound => _localization.GetLocalizedString("AmbassadorNotFound");
        public string MsgAmbassadorRemoved => _localization.GetLocalizedString("AmbassadorRemoved");
        public string MsgNoActiveAmbassador => _localization.GetLocalizedString("NoActiveAmbassador");
        public string MsgCurrentAmbassadorRetrieved => _localization.GetLocalizedString("CurrentAmbassadorRetrieved");
        public string MsgAmbassadorHistoryEmpty => _localization.GetLocalizedString("AmbassadorHistoryEmpty");
        public string MsgAmbassadorHistorySuccess => _localization.GetLocalizedString("AmbassadorHistorySuccess");
        public string MsgAmbassadorAlreadyActive => _localization.GetLocalizedString("AmbassadorAlreadyActive");
        public string MsgInvalidAmbassadorDuration => _localization.GetLocalizedString("InvalidAmbassadorDuration");
        public string MsgDurationMin => _localization.GetLocalizedString("DurationMin");
        public string MsgDurationMax => _localization.GetLocalizedString("DurationMax");
        public string MsgReasonTooLong => _localization.GetLocalizedString("ReasonTooLong");
        public string MsgClubPostIdInvalid => _localization.GetLocalizedString("ClubPostIdInvalid");
        public string MsgContentRequired => _localization.GetLocalizedString("ContentRequired");
        public string MsgContentMaxLength => _localization.GetLocalizedString("ContentMaxLength");
        public string MsgMediaFileTooLong => _localization.GetLocalizedString("MediaFileTooLong");
        public string MsgInvalidMediaType => _localization.GetLocalizedString("InvalidMediaType");
        public string MsgPostIdRequired => _localization.GetLocalizedString("PostIdRequired");
        public string MsgInvalidPostStatus => _localization.GetLocalizedString("InvalidPostStatus");
        public string MsgRejectionReasonTooLong => _localization.GetLocalizedString("RejectionReasonTooLong");
        public string MsgContentOrMediaRequired => _localization.GetLocalizedString("ContentOrMediaRequired");
        public string MsgMediaTypeRequired => _localization.GetLocalizedString("MediaTypeRequired");
    }
}
