namespace Lumora.Application.Extensions
{
    public static class IFormFileExtensions
    {
        public static ValidatedFileResult PrepareValidatedFile(this IFileStream file, Domain.Enums.MediaType expectedType, FileValidatorHelper validator)
        {
            var (isValid, errorMessage, fileErrorType) = validator.Validate(file, expectedType);
            if (!isValid)
            {
                return new ValidatedFileResult(false, errorMessage, null, null, null, null, null, fileErrorType);
            }

            var originalName = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var cleanName = originalName.Slugify();
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var uniqueName = $"{cleanName}_{uniqueId}{extension}";
            var stream = file.OpenReadStream();

            return new ValidatedFileResult(true, null, uniqueName, file.ContentType, file.Length, stream, null);
        }
    }
}
