namespace Lumora.Domain.Exceptions
{
    public class CustomException : Exception
    {
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public object? AdditionalData { get; set; }

        public override string ToString()
        {
            return $"Error Code: {ErrorCode}, Error Message: {ErrorMessage}";
        }

        public CustomException(string message, string? errorCode = null, object? additionalData = null)
            : base(message)
        {
            ErrorCode = errorCode;
            AdditionalData = additionalData;
        }

        public CustomException(string message, Exception innerException, string? errorCode = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
