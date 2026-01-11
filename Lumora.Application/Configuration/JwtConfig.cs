namespace Lumora.Application.Configuration
{
    public class JwtConfig
    {
        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public string Secret { get; set; } = string.Empty;

        public int RefreshTokenExpirationDays { get; set; } = 30;
        public int AccessTokenExpirationMinutes { get; set; } = 15;
    }
}
