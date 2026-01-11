namespace Lumora.Infrastructure.Repositories
{
    public class StaticContentRepository(PgDbContext dbContext) : IStaticContentRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<StaticContent?> GetByKeyAsync(string key, string language, bool onlyActive = true)
        {
            var query = _dbContext.StaticContents.AsQueryable();
            if (onlyActive) query = query.Where(c => c.IsActive);

            return await query.FirstOrDefaultAsync(c => c.Key == key && c.Language == language);
        }

        public async Task<List<StaticContent>> GetByKeysAsync(IEnumerable<string> keys, string language, bool onlyActive = true)
        {
            var query = _dbContext.StaticContents.Where(c => keys.Contains(c.Key) && c.Language == language);
            if (onlyActive) query = query.Where(c => c.IsActive);

            return await query.ToListAsync();
        }

        public async Task<List<StaticContent>> GetAllAsync(string? group = null, string? language = null, bool? isActive = true)
        {
            var query = _dbContext.StaticContents.AsQueryable();

            if (!string.IsNullOrEmpty(group)) query = query.Where(c => c.Group == group);
            if (!string.IsNullOrEmpty(language)) query = query.Where(c => c.Language == language);
            if (isActive.HasValue) query = query.Where(c => c.IsActive == isActive.Value);

            return await query.OrderBy(c => c.Key).ToListAsync();
        }

        public async Task AddAsync(StaticContent content) => await _dbContext.StaticContents.AddAsync(content);

        public Task UpdateAsync(StaticContent content)
        {
            _dbContext.StaticContents.Update(content);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
    }
}
