namespace Lumora.Application.Configuration
{
    // Otp Verification Options 
    public class OtpVerificationOptions
    {
        // Enable or disable in-memory cache
        public bool IsInMemoryCache { get; set; }

        // Active to generate URL to verify code with Id OTP
        public bool EnableUrl { get; set; }
        public int Iterations { get; set; }
        public int Size { get; set; }
        public int Length { get; set; }
        public int Expire { get; set; }
        public string? BaseOtpUrl { get; set; }
    }
}
