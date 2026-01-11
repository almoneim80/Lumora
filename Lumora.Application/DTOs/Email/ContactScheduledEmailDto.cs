namespace Lumora.Application.DTOs.Email
{
    public class ContactScheduledEmailDto
    {
        public string? Cron { get; set; } = string.Empty;

        public string? Day { get; set; } = string.Empty;

        public TimeOnly? Time { get; set; }
    }
}
