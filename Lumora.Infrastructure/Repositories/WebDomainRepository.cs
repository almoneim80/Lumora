using Microsoft.EntityFrameworkCore;

namespace Lumora.Infrastructure.Repositories
{
    public class WebDomainRepository(PgDbContext dbContext) : IDomainRepository
    {
        public IQueryable<WebDomain> GetDomains() => dbContext.Domains!;
        public async Task<WebDomain?> GetByNameAsync(string name) => await dbContext.Domains!.FirstOrDefaultAsync(d => d.Name == name);
        public async Task<List<WebDomain>> GetByNamesAsync(List<string> names) => await dbContext.Domains!.Where(d => names.Contains(d.Name)).ToListAsync();
        public async Task AddAsync(WebDomain domain) => await dbContext.Domains!.AddAsync(domain);
        public void Update(WebDomain domain) => dbContext.Domains!.Update(domain);
        public void AddRange(IEnumerable<WebDomain> domains) => dbContext.Set<WebDomain>().AddRange(domains);
        public void UpdateRange(IEnumerable<WebDomain> domains) => dbContext.Set<WebDomain>().UpdateRange(domains);
        public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync(true, default);
    }
}
