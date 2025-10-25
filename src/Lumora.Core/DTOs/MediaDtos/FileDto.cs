namespace Lumora.DTOs.MediaDtos;

public class ValidatedFileResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }
    public string? UniqueName { get; }
    public string? MimeType { get; }
    public long? FileSize { get; }
    public Stream? Stream { get; }
    public string? TemporaryPath { get; }
    public FileValidationErrorType? FileErrorType { get; }

    public ValidatedFileResult(
        bool isValid,
        string? errorMessage,
        string? uniqueName,
        string? mimeType,
        long? fileSize,
        Stream? stream = null,
        string? temporaryPath = null,
        FileValidationErrorType? fileErrorType = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        UniqueName = uniqueName;
        MimeType = mimeType;
        FileSize = fileSize;
        Stream = stream;
        TemporaryPath = temporaryPath;
        FileErrorType = fileErrorType;
    }
}
