namespace Lumora.Application.Services.WebDomainSvc
{
    public class WebDomainService(
    IDomainRepository repository,
    IWebCheckerService webChecker,
    IMxVerifyService mxVerifyService,
    ILogger<WebDomainService> logger,
    IMapper mapper) : IWebDomainService
    {
        public async Task Verify(WebDomain domain)
        {
            webChecker.VerifyFreeAndDisposable(domain);

            if (domain.DnsCheck == null) await webChecker.VerifyDns(domain);

            if (domain.DnsCheck is true)
            {
                if (domain.HttpCheck == null) await webChecker.VerifyHttp(domain);
                if (domain.MxCheck == null) await VerifyMX(domain);
            }
            else
            {
                domain.HttpCheck = false;
                domain.MxCheck = false;
                domain.Url = domain.Title = domain.Description = null;
            }
        }

        public async Task SaveAsync(WebDomain domain)
        {
            webChecker.VerifyFreeAndDisposable(domain);
            if (domain.Id > 0) repository.Update(domain);
            else await repository.AddAsync(domain);
        }

        public async Task SaveRangeAsync(List<WebDomain> domains)
        {
            domains.ForEach(webChecker.VerifyFreeAndDisposable);

            var groups = domains.GroupBy(d => d.Id > 0);
            foreach (var group in groups)
            {
                if (group.Key) repository.UpdateRange(group);
                else repository.AddRange(group);
            }
            // إضافة await هنا تجعل استخدام async في توقيع الدالة صحيحاً
            await repository.SaveChangesAsync();
        }

        public string GetDomainNameByEmail(string email)
        {
            try
            {
                return string.IsNullOrEmpty(email) ? "" : new MailAddress(email).Host;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return string.Empty;
            }
        }

        public async Task<DomainDetailsDto> VerifyDomainAsync(string name, bool force)
        {
            var domain = repository.GetDomains().FirstOrDefault(d => d.Name == name);

            if (domain == null)
            {
                domain = new WebDomain { Name = name };
                await SaveAsync(domain);
                await repository.SaveChangesAsync();
            }

            if (force)
            {
                domain.Title = null;
                domain.Description = null;
                domain.DnsRecords = null;
                domain.DnsCheck = null;
                domain.HttpCheck = null;
                domain.MxCheck = null;
                domain.Url = null;
            }

            await Verify(domain);
            await repository.SaveChangesAsync();

            return mapper.Map<DomainDetailsDto>(domain);
        }

        private async Task VerifyMX(WebDomain domain)
        {
            domain.MxCheck = false;

            // طلب السجلات من الـ Infrastructure بدلاً من استخدام lookupClient هنا
            var mxRecordValues = await webChecker.GetMxRecordsAsync(domain.Name);

            foreach (var mxRecordValue in mxRecordValues)
            {
                var mxVerify = await mxVerifyService.Verify(mxRecordValue);
                if (mxVerify)
                {
                    domain.MxCheck = true;
                    break;
                }
            }
        }
    }
}
