using System.Net;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Serilog;
using Lumora.Configuration;

namespace Lumora.Services;

public class EmailService : IEmailService
{
    private readonly EmailConfig config = new EmailConfig();
    public EmailService(IConfiguration configuration)
    {
        var settings = configuration.GetSection("Email").Get<EmailConfig>();

        if (settings != null)
        {
            config = settings;
        }
        else
        {
            // throw an exception if the email settings are not found
            throw new MissingConfigurationException($"The specified configuration section for the type {typeof(EmailConfig).FullName} could not be found in the settings file.");
        }
    }

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
            Log.Error(exception, "Error sending emails");

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

    // Special function to create the content of the email message
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
