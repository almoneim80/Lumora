namespace Lumora.Interfaces;

public interface IBaseServiceWithoutUpdate<T>
    where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetOneAsync(int id);
    Task<(List<T> Items, int TotalCount)> GetPagedAsync(int? page = null, int? pageSize = null);
    Task<T> CreateAsync(T entity);
    Task<bool> DeleteAsync(int id);

    Task<List<T>> ExportToCsvAsync();
    Task<List<T>> ExportToExcelAsync();
    Task<List<T>> ExportToJsonAsync();
}
