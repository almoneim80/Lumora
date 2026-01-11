namespace Lumora.Plugin.Sms.DTOs;
public class OtpOptions
{
    public int ExpireInMinutes { get; set; } = 5;
    public int ResendCooldownSeconds { get; set; } = 60;
    public string SmsTemplate { get; set; } = "Your verification code is {otp}. It will expire in {minutes} minutes.";

    public int MaxFailedAttempts { get; set; } = 5;
    public int BlockDurationMinutes { get; set; } = 5;
    public int FailedAttemptWindowMinutes { get; set; } = 10;
}
