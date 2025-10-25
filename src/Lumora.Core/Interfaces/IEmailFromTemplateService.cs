namespace Lumora.Interfaces;

// واجهة لخدمة إرسال البريد الإلكتروني باستخدام القوالب
public interface IEmailFromTemplateService
{
    // إرسال بريد إلكتروني باستخدام قالب إلى عدة مستلمين
    Task SendAsync(string templateName, string language, string[] recipients, Dictionary<string, string>? templateArguments, List<AttachmentDto>? attachments);

    // إرسال بريد إلكتروني باستخدام قالب إلى جهة اتصال محددة
    Task SendToContactAsync(int contactId, string templateName, Dictionary<string, string>? templateArguments, List<AttachmentDto>? attachments, int scheduleId = 0);
}
