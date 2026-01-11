namespace Lumora.Domain.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTimeOffset GetDateTime(double timestamp)
        {
            return DateTimeOffset.UnixEpoch.AddSeconds(timestamp).ToUniversalTime();
        }

        public static double GetTimeStamp(DateTimeOffset dateTime)
        {
            return (int)dateTime.Subtract(DateTimeOffset.UnixEpoch).TotalSeconds;
        }
    }
}
