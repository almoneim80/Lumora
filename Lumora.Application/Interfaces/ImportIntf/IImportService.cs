using Lumora.Application.DTOs.Import;

namespace Lumora.Application.Interfaces.ImportIntf
{
    public interface IImportService<T, TI> where TI : BaseEntityWithId where T : BaseEntityWithId, new()
    {
        Task<ImportResult> ImportFromListAsync(List<TI> importRecords);
        Task<ImportResult> ImportFromFileAsync(IFileStream file);
    }
}
