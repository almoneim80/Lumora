namespace Lumora.Interfaces;

public interface IImportService<T, TI>
    where TI : BaseEntityWithId
    where T : BaseEntityWithId, new()
{
    Task<ImportResult> ImportFromListAsync(List<TI> importRecords);
    Task<ImportResult> ImportFromFileAsync(IFormFile file);
    Task<byte[]> GenerateCsvTemplateAsync<TDto>() where TDto : class;
    Task<byte[]> GenerateExcelTemplateAsync<TDto>() where TDto : class;
}
