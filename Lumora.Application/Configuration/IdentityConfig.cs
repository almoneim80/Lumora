namespace Lumora.Application.Configuration
{
    public class IdentityConfig
{
    public double LockoutTime { get; set; } = 15;

    public int MaxFailedAccessAttempts { get; set; } = 5;
}
}
