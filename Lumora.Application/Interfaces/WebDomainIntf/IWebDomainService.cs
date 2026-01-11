namespace Lumora.Application.Interfaces.WebDomainIntf
{
    public interface IWebDomainService : IEntityService<WebDomain>
    {
        public Task Verify(WebDomain domain);
        public string GetDomainNameByEmail(string email);
        Task<DomainDetailsDto> VerifyDomainAsync(string name, bool force);
    }
}
