namespace Lumora.Enums;

public enum NotificationType
{
    [Description("General")]
    General = 1,

    [Description("Payment")]
    Payment = 2,

    [Description("Message")]
    Message = 3,

    [Description("Alert")]
    Alert = 4,

    [Description("System")]
    System = 5,

    [Description("Error")]
    Error = 6,

    [Description("Warning")]
    Warning = 7,
}
