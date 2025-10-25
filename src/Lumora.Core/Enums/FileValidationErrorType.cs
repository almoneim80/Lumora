namespace Lumora.Enums
{
    public enum FileValidationErrorType
    {
        None = 0,
        FileMissing = 1,
        UnsupportedExtension = 2,
        InvalidMimeType = 3,
        ExceedsMaxSize = 4,
        InvalidSizeFormat = 5,
        UnsupportedCategory = 6
    }
}
