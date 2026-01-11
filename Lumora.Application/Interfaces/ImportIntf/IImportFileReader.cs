namespace Lumora.Application.Interfaces.ImportIntf
{
    public interface IImportFileReader
    {
        // دالة لاستخراج البيانات من الملف وتحويلها لقائمة من نوع TI
        Task<List<TI>> ReadFileAsync<TI>(Stream fileStream, string fileExtension) where TI : new();

        // دالتين لتوليد القوالب (لأن الخدمة الحالية تقوم بذلك أيضاً)
        Task<byte[]> GenerateTemplateAsync<TDto>(string format) where TDto : class;
    }
}
