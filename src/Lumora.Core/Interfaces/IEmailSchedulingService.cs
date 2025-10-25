namespace Lumora.Interfaces;

// واجهة لخدمة جدولة البريد الإلكتروني
public interface IEmailSchedulingService
{
    // البحث عن جدول البريد الإلكتروني بناءً على اسم المجموعة ورمز اللغة
    Task<EmailSchedule?> FindByGroupAndLanguage(string groupName, string languageCode);

    // تعيين سياق قاعدة البيانات
    void SetDBContext(PgDbContext pgDbContext);
}
