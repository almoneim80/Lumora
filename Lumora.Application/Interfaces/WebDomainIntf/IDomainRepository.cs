namespace Lumora.Application.Interfaces.WebDomainIntf
{
    public interface IDomainRepository
    {
        IQueryable<WebDomain> GetDomains();
        Task AddAsync(WebDomain domain);
        void Update(WebDomain domain);
        void AddRange(IEnumerable<WebDomain> domains);
        void UpdateRange(IEnumerable<WebDomain> domains);
        Task SaveChangesAsync();
        Task<WebDomain?> GetByNameAsync(string name);
        Task<List<WebDomain>> GetByNamesAsync(List<string> names);
    }
}
