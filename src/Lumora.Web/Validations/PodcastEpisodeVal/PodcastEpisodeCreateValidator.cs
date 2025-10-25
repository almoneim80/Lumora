using Lumora.DTOs.Podcast;

namespace Lumora.Web.Validations.PodcastEpisodeVal
{
    public class PodcastEpisodeCreateValidator : AbstractValidator<PodcastEpisodeCreateDto>
    {
        public PodcastEpisodeCreateValidator(PodcastEpisodeMessage messages)
        {
            RuleFor(x => x.Title)
                .NotNull()
                .WithMessage(messages.MsgPodcastTitleRequired);

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(messages.MsgPodcastTitleRequired);

            RuleFor(x => x.Title)
                .MaximumLength(250)
                .WithMessage(messages.MsgPodcastTitleTooLong);

            RuleFor(x => x.EpisodeNumber)
                .GreaterThan(0)
                .WithMessage(messages.MsgPodcastEpisodeNumberInvalid);

            RuleFor(x => x.YoutubeUrl)
                .NotNull()
                .WithMessage(messages.MsgYoutubeUrlRequired);

            RuleFor(x => x.YoutubeUrl)
                .NotEmpty()
                .WithMessage(messages.MsgYoutubeUrlRequired);

            RuleFor(x => x.YoutubeUrl)
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage(messages.MsgYoutubeUrlInvalid);

            RuleFor(x => x.ThumbnailUrl)
                .NotEmpty()
                .WithMessage(messages.MsgThumbnailUrlRequired);

            RuleFor(x => x.ThumbnailUrl)
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage(messages.MsgThumbnailUrlInvalid);
        }
    }
}
