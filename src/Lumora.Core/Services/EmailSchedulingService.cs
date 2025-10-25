using Microsoft.Extensions.Options;
using Lumora.Configuration;

namespace Lumora.Services;

// تنفيذ خدمة جدولة البريد الإلكتروني
public class EmailSchedulingService : IEmailSchedulingService
{
    private readonly IOptions<ApiSettingsConfig> apiSettingsConfig;
    private PgDbContext dbContext;

    // المنشئ: حقن سياق قاعدة البيانات وإعدادات API
    public EmailSchedulingService(PgDbContext dbContext, IOptions<ApiSettingsConfig> apiSettingsConfig)
    {
        this.dbContext = dbContext;
        this.apiSettingsConfig = apiSettingsConfig;
    }

    // تنفيذ دالة البحث عن جدول البريد الإلكتروني
    public async Task<EmailSchedule?> FindByGroupAndLanguage(string groupName, string languageCode)
    {
        EmailSchedule? result;

        // تحقق مما إذا كانت جهة الاتصال.Language بصيغة مكونة من حرفين واضبط الاستعلام وفقًا لذلك
        // استعلام أساسي للبحث عن جداول البريد الإلكتروني
        var emailSchedulesQuery = dbContext.EmailSchedules!
            .Include(c => c.Group)
            .Where(e => e.Group!.Name == groupName);

        // التعامل مع رموز اللغة ثنائية الحروف
        if (languageCode.Length == 2)
        {
            result = await emailSchedulesQuery.FirstOrDefaultAsync(e => e.Group!.Language.StartsWith(languageCode));
        }
        else
        {
            // البحث عن تطابق دقيق أولاً
            result = await emailSchedulesQuery.FirstOrDefaultAsync(e => e.Group!.Language == languageCode);

            // إذا لم يتم العثور على تطابق دقيق، جرب البحث باستخدام الجزء الأول من رمز اللغة
            if (result == null)
            {
                var lang = languageCode.Split('-')[0];

                result = await emailSchedulesQuery.FirstOrDefaultAsync(e => e.Group!.Language.StartsWith(lang));
            }
        }

        // إذا لم يتم العثور على نتيجة، استخدم اللغة الافتراضية
        if (result == null)
        {
            result = await emailSchedulesQuery.FirstOrDefaultAsync(e => e.Group!.Language == apiSettingsConfig.Value.DefaultLanguage);
        }

        return result;
    }

    // تنفيذ دالة تعيين سياق قاعدة البيانات
    public void SetDBContext(PgDbContext pgDbContext)
    {
        dbContext = pgDbContext;
    }
}
