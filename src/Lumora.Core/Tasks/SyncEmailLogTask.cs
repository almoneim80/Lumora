﻿using Serilog;
using Lumora.Configuration;
namespace Lumora.Tasks
{
    // هذا الكلاس SyncEmailLogTask هو مهمة لمزامنة سجلات البريد الإلكتروني مع سجل النشاط.
    // هذه المهمة تعتبر جزءًا مهمًا من نظام إدارة البريد الإلكتروني والتسويق،
    // حيث تساعد في الحفاظ على سجل دقيق ومحدث لجميع التفاعلات عبر البريد الإلكتروني مع جهات الاتصال.
    public class SyncEmailLogTask : BaseTask
    {
        private static readonly string SourceName = "EmailService";
        private readonly PgDbContext dbContext;
        private readonly ActivityLogService logService;
        private readonly IServiceProvider provider;
        private readonly int batchSize;

        // المنشئ: يقوم بتهيئة المهمة وتحميل الإعدادات
        public SyncEmailLogTask(IConfiguration configuration, PgDbContext dbContext, TaskStatusService taskStatusService, ActivityLogService logService, IServiceProvider provider)
            : base("Tasks:SyncEmailLogTask", configuration, taskStatusService)
        {
            this.dbContext = dbContext;
            this.logService = logService;
            this.provider = provider;

            // تحميل حجم الدفعة من الإعدادات
            var config = configuration.GetSection(configKey)!.Get<TaskWithBatchConfig>();
            if (config is not null)
            {
                batchSize = config.BatchSize;
            }
            else
            {
                throw new MissingConfigurationException($"The specified configuration section for the provided configKey {configKey} could not be found in the settings file.");
            }
        }

        // تنفيذ المهمة
        public async override Task<bool> Execute(TaskExecutionLog currentJob)
        {
            try
            {
                using (var scope = provider.CreateScope())
                {
                    // الحصول على أحدث معرف تمت معالجته
                    var contactService = scope.ServiceProvider.GetRequiredService<ContactService>();

                    var maxId = await logService.GetMaxId(SourceName);

                    // استرداد دفعة جديدة من سجلات البريد الإلكتروني
                    var batch = await dbContext.EmailLogs!.Where(e => e.Id > maxId).OrderBy(e => e.Id).Take(batchSize).ToListAsync();

                    // استرداد جهات الاتصال الموجودة
                    var batchContacts = batch.Select(b => b.Recipients).ToArray();

                    // تحويل سجلات البريد الإلكتروني إلى سجلات نشاط
                    var existingContacts = await dbContext.Contacts!.Where(contact => batchContacts.Contains(contact.Email)).ToDictionaryAsync(k => k.Email);

                    var converted = batch.Select(log =>
                    {
                        Contact? contact;

                        if (!existingContacts.TryGetValue(log.Recipients, out contact))
                        {
                            // إنشاء جهة اتصال جديدة إذا لم تكن موجودة
                            contact = new Contact
                            {
                                Email = log.Recipients,
                            };

                            contactService.SaveAsync(contact).Wait();
                        }

                        // إنشاء سجل نشاط جديد
                        return new ActivityLog()
                        {
                            Source = SourceName,
                            SourceId = log.Id,
                            Type = "Message",
                            ContactId = contact.Id,
                            CreatedAt = log.CreatedAt,
                            Data = JsonHelper.Serialize(new { Id = log.Id, Status = log.Status, Sender = log.FromEmail, Recipient = log.Recipients, Subject = log.Subject }),
                        };
                    }).ToList();

                    await dbContext.SaveChangesAsync();
                    var res = await logService.AddActivityRecords(converted);
                    if (!res)
                    {
                        throw new SyncEmailLogTaskException("Unable to log email events");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to dump email events to activity log. Reason: " + ex.Message);
                throw;
            }
        }
    }
}
