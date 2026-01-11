namespace Lumora.Application.Interfaces.BaseIntf
{
    public interface IBaseService<T> where T : class
    {
        Task<T?> GetOneAsync(int id);
        Task<(List<T> Items, int TotalCount)> GetPagedAsync(int? page = null, int? pageSize = null);
        Task<T> CreateAsync(T entity);
        Task<T?> UpdateAsync(int id, T entity);
        Task<bool> DeleteAsync(int id);
    }
}
