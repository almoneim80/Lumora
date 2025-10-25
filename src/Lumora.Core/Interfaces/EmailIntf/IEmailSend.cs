namespace Lumora.Interfaces.Email
{
    public interface IEmailSend
    {
        /// <summary>
        /// Sends an email with the specified details.
        /// </summary>
        Task<string> SendAsync(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments);

        /// <summary>
        /// Sends an email with the specified details.
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
