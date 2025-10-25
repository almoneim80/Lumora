namespace Lumora.Exceptions;

public class CustomException : Exception
{
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        return $"Error Code: {ErrorCode}, Error Message: {ErrorMessage}";
    }

    public CustomException(Microsoft.Extensions.Logging.ILogger logger, string message, Exception innerException)
    : base(message, innerException)
    {
        logger.LogError(innerException, message);
    }

    public CustomException(Microsoft.Extensions.Logging.ILogger logger, string message, object additionalData, Exception innerException)
        : base(message, innerException)
    {
        logger.LogError(innerException, message, additionalData);
    }
}
