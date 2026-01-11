namespace Lumora.Application.Services
{
    public class ContactService(
        IContactRepository contactRepository, IWebDomainService domainService,
        IEmailSchedulingService emailSchedulingService, IOptions<ApiSettingsConfig> apiSettingsConfig,
        IDomainRepository domainRepository, IUnitOfWork unitOfWork) : IContactService
    {
        private readonly IWebDomainService _domainService = domainService;
        private readonly IEmailSchedulingService _emailSchedulingService = emailSchedulingService;
        private readonly IOptions<ApiSettingsConfig> _apiSettingsConfig = apiSettingsConfig;
        private readonly IContactRepository _contactRepository = contactRepository;
        private readonly IDomainRepository _domainRepository = domainRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task SaveAsync(Contact contact)
        {
            await EnrichWithDomainId(contact);
            EnrichWithAccountId(contact);
            await _contactRepository.UpsertAsync(contact);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SaveRangeAsync(List<Contact> contacts)
        {
            await EnrichWithDomainIdAsync(contacts);
            EnrichWithAccountId(contacts);

            await _contactRepository.UpsertRangeAsync(contacts);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Contact> FindOrCreate(string email, string language, int timezone)
        {
            var customer = await _contactRepository.GetByEmailAsync(email);

            if (customer == null)
            {
                customer = new Contact { Email = email };
            }

            customer.Timezone = timezone;
            customer.Language = language;

            await SaveAsync(customer);
            return customer;
        }

        public async Task Subscribe(Contact contact, string groupName)
        {
            var language = contact.Language ?? _apiSettingsConfig.Value.DefaultLanguage;
            var emailSchedule = await _emailSchedulingService.FindByGroupAndLanguage(groupName, language);

            if (emailSchedule == null)
            {
                throw new EntityNotFoundException(typeof(EmailSchedule).Name, groupName);
            }

            await _contactRepository.AddContactScheduleAsync(new ContactEmailSchedule
            {
                Contact = contact,
                Schedule = emailSchedule,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }

        public async Task Unsubscribe(string email, string reason, string source, DateTimeOffset createdAt, string? ip)
        {
            var contact = await _contactRepository.GetByEmailAsync(email);

            if (contact != null)
            {
                var unsubscribe = new Unsubscribe
                {
                    ContactId = contact.Id,
                    Reason = reason,
                    CreatedByIp = ip,
                    Source = source,
                    CreatedAt = createdAt,
                };

                await _contactRepository.AddUnsubscribeAsync(unsubscribe);
                // تحديث حالة الجدولة عبر المستودع
                await _contactRepository.UpdateSchedulesStatusAsync(contact.Id, ScheduleStatus.Pending, ScheduleStatus.Unsubscribed);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task EnrichWithDomainId(Contact contact)
        {
            var domainName = _domainService.GetDomainNameByEmail(contact.Email);
            var domain = await _domainRepository.GetByNameAsync(domainName);

            if (domain != null)
            {
                contact.DomainId = domain.Id;
                contact.Domain = domain;
            }
            else
            {
                var newDomain = new WebDomain
                {
                    Name = domainName,
                    AccountStatus = AccountSyncStatus.NotInitialized
                };
                await _domainService.SaveAsync(newDomain);
                contact.Domain = newDomain;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task EnrichWithDomainIdAsync(List<Contact> contacts)
        {
            var newDomains = new Dictionary<string, WebDomain>();

            // 1. استخراج كل الدومينات الفريدة من الإيميلات المطلوبة
            var domainNames = contacts.Select(c => _domainService.GetDomainNameByEmail(c.Email)).Distinct().ToList();

            // 2. جلب الدومينات الموجودة فعلياً من قاعدة البيانات بضربة واحدة (عبر المستودع)
            var existingDomainsInDb = await _domainRepository.GetByNamesAsync(domainNames);

            foreach (var contact in contacts)
            {
                var domainName = _domainService.GetDomainNameByEmail(contact.Email);
                var domain = existingDomainsInDb.FirstOrDefault(d => d.Name == domainName);

                if (domain != null)
                {
                    contact.DomainId = domain.Id;
                    contact.Domain = domain;
                }
                else
                {
                    // إذا لم يكن موجوداً، نتحقق من القائمة الجديدة التي أنشأناها في هذه الدورة
                    if (!newDomains.TryGetValue(domainName, out var brandNewDomain))
                    {
                        brandNewDomain = new WebDomain()
                        {
                            Name = domainName,
                            Source = contact.Email,
                            AccountStatus = AccountSyncStatus.NotIntended,
                        };
                        newDomains.Add(domainName, brandNewDomain);
                        await _domainService.SaveAsync(brandNewDomain);
                    }
                    contact.Domain = brandNewDomain;
                    contact.DomainId = brandNewDomain.Id;
                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

        private void EnrichWithAccountId(List<Contact> contacts)
        {
            foreach (var contact in contacts)
            {
                var domain = contact.Domain;
                if (domain != null)
                {
                    contact.AccountId = domain.AccountId;
                }
            }
        }

        private void EnrichWithAccountId(Contact contact)
        {
            var domain = contact.Domain;
            if (domain != null)
            {
                contact.AccountId = domain.AccountId;
            }
        }
    }
}
