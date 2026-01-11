namespace Lumora.Infrastructure.Files
{
    public class FormFileWrapper : IFileStream
    {
        private readonly IFormFile _file;
        public FormFileWrapper(IFormFile file)
        {
            _file = file;
        }

        public string FileName => _file.FileName;
        public string ContentType => _file.ContentType;
        public long Length => _file.Length;
        public Stream OpenReadStream() => _file.OpenReadStream();
    }
}
