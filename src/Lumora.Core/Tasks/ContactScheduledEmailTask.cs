using Serilog;
using System.Text.Json;

namespace Lumora.Tasks;

//هذا الكلاس ContactScheduledEmailTask هو مهمة مجدولة لإرسال رسائل البريد الإلكتروني إلى جهات الاتصال.
public class ContactScheduledEmailTask : BaseTask
{
    private readonly PgDbContext dbContext;
    private readonly IEmailFromTemplateService emailFromTemplateService;

    // المنشئ: يحقن الخدمات اللازمة
    public ContactScheduledEmailTask(PgDbContext dbContext, IEmailFromTemplateService emailFromTemplateService, IConfiguration configuration, TaskStatusService taskStatusService)
        : base("Tasks:ContactScheduledEmail", configuration, taskStatusService)
    {
        this.dbContext = dbContext;
        this.emailFromTemplateService = emailFromTemplateService;
    }

    // الدالة الرئيسية لتنفيذ المهمة
    public override async Task<bool> Execute(TaskExecutionLog currentJob)
    {
        try
        {
            // Load all the contact schedules in ContactEmailSchedule table by pending status
            // تحميل جميع جداول الاتصال المعلقة
            var schedules = dbContext.ContactEmailSchedules!
                .Include(c => c.Schedule)
                .Include(c => c.Contact)
                .Where(s => s.Status == ScheduleStatus.Pending)
                .ToList();

            foreach (var schedule in schedules)
            {
                try
                {
                    // التحقق من إلغاء الاشتراك
                    if (schedule.Contact?.UnsubscribeId is not null)
                    {
                        schedule.Status = ScheduleStatus.Unsubscribed;
                        await dbContext.SaveChangesAsync();
                        continue;
                    }

                    // تحديد القالب التالي للإرسال ومنطق إعادة المحاولة
                    EmailTemplate? nextEmailTemplateToSend;
                    var retryDelay = 0;

                    var lastEmailLog = dbContext.EmailLogs!.Where(
                            e => e.ScheduleId == schedule.ScheduleId &&
                            e.ContactId == schedule.ContactId).OrderByDescending(x => x.CreatedAt).FirstOrDefault();

                    var lastEmailTemplate = lastEmailLog is not null
                        ? dbContext.EmailTemplates!.FirstOrDefault(e => e.Id == lastEmailLog!.TemplateId)
                        : null;

                    // منطق إعادة المحاولة إذا لم يتم إرسال البريد الإلكتروني الأخير
                    if (lastEmailLog is not null && lastEmailLog!.Status is EmailStatus.NotSent)
                    {
                        var emailNotSentCount = dbContext.EmailLogs!.Count(
                                e => e.ScheduleId == schedule.ScheduleId
                                && e.ContactId == schedule.ContactId
                                && e.TemplateId == lastEmailLog.TemplateId
                                && e.Status == EmailStatus.NotSent);

                        // إذا اكتملت جميع محاولات إعادة المحاولة، احصل على قالب البريد الإلكتروني التالي لإرساله.
                        if (emailNotSentCount > lastEmailTemplate!.RetryCount)
                        {
                            nextEmailTemplateToSend = GetNextEmailTemplateToSend(lastEmailTemplate, schedule.Schedule!.GroupId);
                        }
                        else
                        {
                            // محاولة إعادة المحاولة متاحة. إرسال نفس قالب البريد الإلكتروني.
                            nextEmailTemplateToSend = dbContext.EmailTemplates!.FirstOrDefault(t => t.Id == lastEmailTemplate.Id);
                            retryDelay = lastEmailTemplate!.RetryInterval;
                        }
                    }
                    else
                    {
                        nextEmailTemplateToSend = GetNextEmailTemplateToSend(lastEmailTemplate!, schedule.Schedule!.GroupId);
                    }

                    // يتم إرسال جميع رسائل البريد الإلكتروني في الجدول الزمني لجهة الاتصال المحددة.
                    // إذا تم إرسال جميع رسائل البريد الإلكتروني في الجدول
                    if (nextEmailTemplateToSend is null)
                    {
                        schedule.Status = ScheduleStatus.Completed;
                        await dbContext.SaveChangesAsync();

                        continue;
                    }

                    // تحديد وقت التنفيذ التالي
                    var nextExecutionTime = GetNextExecutionTime(schedule, retryDelay, lastEmailLog);

                    if (nextExecutionTime is not null)
                    {
                        // check IsRightTimeToExecute()
                        // التحقق مما إذا كان الوقت المناسب للتنفيذ
                        var executeNow = IsRightTimeToExecute(nextExecutionTime.Value.ToUniversalTime());

                        if (executeNow)
                        {
                            // إرسال البريد الإلكتروني
                            await emailFromTemplateService.SendToContactAsync(schedule.ContactId, nextEmailTemplateToSend!.Name, GetTemplateArguments(), null, schedule.ScheduleId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // معالجة الأخطاء لكل جدول فردي
                    Log.Error(ex, $"Failed to complete email sending for contact schedule Id = {schedule.Id}");
                    schedule.Status = ScheduleStatus.Failed;
                    await dbContext.SaveChangesAsync();
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // معالجة الأخطاء العامة للمهمة
            Log.Error(ex, $"Error occurred when executing contact scheduled task in task runner {currentJob.Id}");
            return false;
        }
    }

    // دوال مساعدة
    // ... (تحديد القالب التالي للإرسال)
    private EmailTemplate? GetNextEmailTemplateToSend(EmailTemplate lastEmailTemplate, int groupId)
    {
        var emailsOfGroup = dbContext.EmailTemplates!.Where(t => t.EmailGroupId == groupId).OrderBy(t => t.Id).ToList();

        var indexOfLastEmail = lastEmailTemplate is not null
            ? emailsOfGroup.IndexOf(lastEmailTemplate)
            : -1;

        if (emailsOfGroup.Count == indexOfLastEmail + 1)
        {
            return null;
        }

        var nextEmailToSend = emailsOfGroup[indexOfLastEmail + 1];

        return nextEmailToSend;
    }

    // ... (الحصول على متغيرات القالب)
    private Dictionary<string, string> GetTemplateArguments()
    {
        // TODO: contact based template arguments
        // Get related variable dictionary from variable service.
        // Add any required scope based variables into the dictionary.
        return new Dictionary<string, string> { { "Key", "Value" } };
    }

    // ... (التحقق مما إذا كان الوقت المناسب للتنفيذ)
    private bool IsRightTimeToExecute(DateTimeOffset nextExecutionTime)
    {
        if (nextExecutionTime <= DateTimeOffset.UtcNow)
        {
            return true;
        }

        return false;
    }

    // ... (حساب وقت التنفيذ التالي بناءً على الجدول والتوقيت الزمني للمستخدم)
    private DateTimeOffset? GetNextExecutionTime(ContactEmailSchedule contactEmailSchedule, int retryDelay, EmailLog? lastEmailLog)
    {
        var contactSchedule = JsonSerializer.Deserialize<Schedule>(contactEmailSchedule.Schedule!.Schedule);
        var userToServerTimeZoneOffset = TimeSpan.FromMinutes(TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes + contactEmailSchedule.Contact!.Timezone!.Value);
        var lastRunTime = lastEmailLog is null ? contactEmailSchedule.CreatedAt : lastEmailLog.CreatedAt;

        // If a retry scenario, adding the retry interval. No need to evaluate schedule.
        if (retryDelay > 0)
        {
            return lastRunTime.ToUniversalTime().AddMinutes(retryDelay);
        }

        // Evaluate CRON based schedule
        if (!string.IsNullOrEmpty(contactSchedule!.Cron))
        {
            var expression = new Quartz.CronExpression(contactSchedule.Cron);

            var nextRunTimeForUser = expression.GetNextValidTimeAfter(new DateTimeOffset(lastRunTime.UtcDateTime, TimeSpan.Zero));
            var nextRunTime = DateTimeOffset.FromUnixTimeMilliseconds(nextRunTimeForUser!.Value.ToUnixTimeMilliseconds()).ToOffset(userToServerTimeZoneOffset);

            return nextRunTime.ToUniversalTime();
        }
        else
        {
            // Evaluate custom schedule based on day and time.

            var days = contactSchedule.Day!.Split(',').Select(int.Parse).ToArray();

            var emailSentCount = dbContext.EmailLogs!.Count(
                            e => e.ScheduleId == contactEmailSchedule.ScheduleId
                            && e.ContactId == contactEmailSchedule.ContactId
                            && e.Status == EmailStatus.Sent);

            // Skip the days already the mail is sent
            var nextRunDate = contactEmailSchedule.Contact!.CreatedAt.HasValue ? contactEmailSchedule.Contact!.CreatedAt.Value.DateTime.Date.AddDays(days[emailSentCount]) : DateTime.MinValue;
            // Add given time in the schedule + user timezone adjustment.
            var nextRunDateTime = new DateTime(nextRunDate.Year, nextRunDate.Month, nextRunDate.Day, contactSchedule!.Time!.Value.Hour, contactSchedule!.Time!.Value.Minute, contactSchedule!.Time!.Value.Second);

            return new DateTimeOffset(nextRunDateTime, userToServerTimeZoneOffset).ToUniversalTime();
        }
    }
}

// فئة مساعدة لتمثيل الجدول
public class Schedule
{
    public string? Cron { get; set; } = string.Empty;

    public string? Day { get; set; } = string.Empty;

    public TimeOnly? Time { get; set; }
}
