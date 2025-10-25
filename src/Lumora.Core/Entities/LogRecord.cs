﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lumora.Entities;

[Table("log_record")]
public class LogRecord
{
    [DataMember(Name = "@timestamp")]
    public DateTime DateTime { get; set; }

    [DataMember(Name = "level")]
    public LogLevel LogLevel { get; set; }

    [DataMember(Name = "message")]
    public string Message { get; set; } = string.Empty;
}
