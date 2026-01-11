namespace Lumora.Plugin.Sms.Exceptions
{
    [Serializable]
    public class UnknownCountryCodeException : Exception
    {
        public UnknownCountryCodeException()
        {
        }

        public UnknownCountryCodeException(string? message)
            : base(message)
        {
        }

        public UnknownCountryCodeException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
