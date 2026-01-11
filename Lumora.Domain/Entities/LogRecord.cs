using System.Runtime.Serialization;

namespace Lumora.Domain.Entities
{
    [Table("log_record")]
    public class LogRecord
    {
        [DataMember(Name = "@timestamp")]
        public DateTime DateTime { get; set; }

        [DataMember(Name = "level")]
        public LogSeverity LogLevel { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; } = string.Empty;
    }
}
