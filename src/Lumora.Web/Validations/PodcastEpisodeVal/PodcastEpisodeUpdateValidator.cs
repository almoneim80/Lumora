using Lumora.DTOs.Podcast;

namespace Lumora.Web.Validations.PodcastEpisodeVal
{
    public class PodcastEpisodeUpdateValidator : AbstractValidator<PodcastEpisodeUpdateDto>
    {
        public PodcastEpisodeUpdateValidator(PodcastEpisodeMessage messages)
        {
            RuleFor(x => x.Title)
                .MaximumLength(250)
                .WithMessage(messages.MsgPodcastTitleTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.EpisodeNumber)
                .GreaterThan(0)
                .WithMessage(messages.MsgPodcastEpisodeNumberInvalid)
                .When(x => x.EpisodeNumber.HasValue);

            RuleFor(x => x.YoutubeUrl)
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage(messages.MsgYoutubeUrlInvalid);

            RuleFor(x => x.ThumbnailUrl)
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage(messages.MsgThumbnailUrlInvalid);
        }
    }
}
