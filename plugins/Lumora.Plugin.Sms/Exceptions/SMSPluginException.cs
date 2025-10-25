namespace Lumora.Plugin.Sms.Exceptions
{
    [Serializable]
    public class SmsPluginException : Exception
    {
        public SmsPluginException()
        {
        }

        public SmsPluginException(string? message)
            : base(message)
        {
        }

        public SmsPluginException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
