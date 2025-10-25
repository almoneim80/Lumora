namespace Lumora.Logging
{
    public static class LoggerExtensions
    {
        public static IDisposable? BeginServiceScope(this ILogger logger, string className, string methodName)
        {
            return logger.BeginScope("[{Class}.{Method}]", className, methodName);
        }

        public static void LogServiceError(this ILogger logger, string className, string methodName, Exception ex, string? customMessage = null, LogEvents eventId = LogEvents.GeneralError)
        {
            logger.LogError(new EventId((int)eventId, $"{className}.{methodName}"), ex, "[{Class}.{Method}] {Message}", className, methodName, customMessage ?? "An error occurred.");
        }

        public static void LogServiceInfo(this ILogger logger, string className, string methodName, string message, LogEvents eventId = LogEvents.Info, params object[] args)
        {
            var allArgs = new object[] { className, methodName }.Concat(args).ToArray();
            logger.LogInformation(new EventId((int)eventId, $"{className}.{methodName}"), "[{Class}.{Method}] " + message, allArgs);
        }

        public static void LogServiceWarning(this ILogger logger, string className, string methodName, string message, LogEvents eventId = LogEvents.Warning, params object[] args)
        {
            var allArgs = new object[] { className, methodName }.Concat(args).ToArray();
            logger.LogWarning(new EventId((int)eventId, $"{className}.{methodName}"), "[{Class}.{Method}] " + message, allArgs);
        }
    }
}
