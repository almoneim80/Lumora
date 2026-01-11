namespace Lumora.Application.Configuration
{
    public class TaskConfig
    {
        public bool Enable { get; set; }

        public string CronSchedule { get; set; } = string.Empty;

        public int RetryCount { get; set; }

        public int RetryInterval { get; set; }
    }
}
