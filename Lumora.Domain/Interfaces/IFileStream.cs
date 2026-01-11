namespace Lumora.Domain.Interfaces
{
    public interface IFileStream
    {
        string FileName { get; }
        string ContentType { get; }
        long Length { get; }
        Stream OpenReadStream();
    }
}
