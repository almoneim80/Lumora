namespace Lumora.Application.Interfaces.SubscribeIntf
{
    public interface IUnsubscribe
    {
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        public string? CreatedByIp { get; set; }

        public string? CreatedById { get; set; }

        public string? CreatedByUserAgent { get; set; }
    }
}
