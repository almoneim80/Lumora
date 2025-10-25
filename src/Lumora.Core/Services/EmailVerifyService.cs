namespace Lumora.Services
{
    public class EmailVerifyService : IEmailVerifyService
    {
        private readonly PgDbContext pgContext;
        private readonly IDomainService domainService;
        private readonly IEmailValidationExternalService emailValidationExternalService;

        public EmailVerifyService(PgDbContext pgDbContext, IDomainService domainService, IEmailValidationExternalService emailValidationExternalService)
        {
            pgContext = pgDbContext;
            this.domainService = domainService;
            this.emailValidationExternalService = emailValidationExternalService;
        }

        public async Task<Domain> Verify(string email)
        {
            // Extracting a domain name from an email
            var domainName = domainService.GetDomainNameByEmail(email);

            // Searching for a domain in the database
            var domain = await (from d in pgContext.Domains
                                where d.Name == domainName
                                select d).FirstOrDefaultAsync();

            if (domain != null && domain.DnsCheck is true)
            {
                return domain;
            }
            else
            {
                // If the domain doesn't exist, create it
                if (domain is null)
                {
                    domain = new Domain() { Name = domainName, Source = email };
                    await domainService.SaveAsync(domain);
                }

                await domainService.Verify(domain!);
                await VerifyDomain(email, domain);
                await pgContext.SaveChangesAsync();

                return domain;
            }
        }

        // Function to check the domain using an external service
        public async Task VerifyDomain(string email, Domain domain)
        {
            var emailVerify = await emailValidationExternalService.Validate(email);
            domain.CatchAll = emailVerify.CatchAllCheck;
        }
    }
}
