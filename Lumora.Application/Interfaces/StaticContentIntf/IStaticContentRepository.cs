namespace Lumora.Application.Interfaces.StaticContentIntf
{
    public interface IStaticContentRepository
    {
        Task<StaticContent?> GetByKeyAsync(string key, string language, bool onlyActive = true);
        Task<List<StaticContent>> GetByKeysAsync(IEnumerable<string> keys, string language, bool onlyActive = true);
        Task<List<StaticContent>> GetAllAsync(string? group = null, string? language = null, bool? isActive = true);
        Task AddAsync(StaticContent content);
        Task UpdateAsync(StaticContent content);
        Task SaveChangesAsync();
    }
}
