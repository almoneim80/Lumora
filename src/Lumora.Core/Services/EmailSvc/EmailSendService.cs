using MailKit;
using MimeKit;
using System.Net;
using System.Security.Authentication;
using Lumora.Interfaces.Email;
namespace Lumora.Services.Email
{
    public class EmailSendService : IEmailSend
    {
        private readonly IConfiguration _configuration;
        private readonly EmailConfig config = new EmailConfig();
        private readonly ILogger<EmailSendService> _logger;
        public EmailSendService(IConfiguration configuration, ILogger<EmailSendService> logger)
        {
            _configuration = configuration;
            var settings = configuration.GetSection("Email").Get<EmailConfig>();

            if (settings != null)
            {
                config = settings;
            }
            else
            {
                // رمي استثناء إذا لم يتم العثور على إعدادات البريد الإلكتروني
                throw new MissingConfigurationException($"The specified configuration section for the type {typeof(EmailConfig).FullName} could not be found in the settings file.");
            }
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> SendAsync(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments)
        {
            var client = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                // الاتصال بخادم SMTP
                await client.ConnectAsync(config.Server, config.Port, config.UseSsl);

                // المصادقة مع الخادم
                await client.AuthenticateAsync(new NetworkCredential(config.UserName, config.Password));

                // إنشاء رسالة البريد الإلكتروني
                var message = await GenerateEmailBody(subject, fromEmail, fromName, recipients, body, attachments);

                // إرسال الرسالة
                await client.SendAsync(message);

                return message.MessageId;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error sending emails");

                switch (exception)
                {
                    case AuthenticationException:
                    case ServiceNotAuthenticatedException:
                        throw new EmailException("Error in authentication", exception);
                    case ServiceNotConnectedException:
                        throw new EmailException("Error connecting to smtp host", exception);
                    default:
                        throw new EmailException("Error sending emails", exception);
                }
            }
            finally
            {
                // قطع الاتصال وتحرير الموارد
                client.Disconnect(true);
                client.Dispose();
            }
        }

        /// <inheritdoc/>
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var fromEmail = _configuration["EmailSender:FromEmail"];
                var fromName = _configuration["EmailSender:FromName"];

                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName))
                {
                    throw new InvalidOperationException("Email settings (FromEmail or FromName) are not configured in appsettings.json.");
                }

                await SendAsync(subject, fromEmail, fromName, new[] { toEmail }, body, null);

                _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}.", toEmail);
                throw;
            }
        }

        // private methods
        private static async Task<MimeMessage> GenerateEmailBody(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments)
        {
            var message = new MimeMessage();
            message.Subject = subject;
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            foreach (var receipent in recipients)
            {
                message.To.Add(MailboxAddress.Parse(receipent));
            }

            var emailBody = new BodyBuilder()
            {
                HtmlBody = body,
            };

            if (attachments is not null)
            {
                foreach (var attachment in attachments)
                {
                    using (var stream = new MemoryStream(attachment.File))
                    {
                        await emailBody.Attachments.AddAsync(attachment.FileName, stream);
                    }
                }
            }

            message.Body = emailBody.ToMessageBody();

            return message;
        }
    }
}
