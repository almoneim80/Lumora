namespace Lumora.Application.Interfaces
{
    public interface IExportableService<T>
    {
        Task<List<T>> ExportToCsvAsync();
        Task<List<T>> ExportToExcelAsync();
        Task<List<T>> ExportToJsonAsync();
    }
}
