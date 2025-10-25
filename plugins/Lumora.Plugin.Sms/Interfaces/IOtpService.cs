namespace Lumora.Plugin.Sms.Interfaces
{
    public interface IOtpService
    {
        /// <summary>
        /// Generates an OTP for a specific user and sends it (via SMS or email).
        /// </summary>
        Task<GeneralResult> GenerateAndSendOtpAsync(string phoneNumber);

        /// <summary>
        /// Resends a previously generated OTP or generates a new one.
        /// </summary>
        Task<GeneralResult> ResendOtpAsync(string phoneNumber);

        /// <summary>
        /// Verifies if the provided OTP is valid for the given user.
        /// </summary>
        Task<GeneralResult<bool>> VerifyOtpAsync(string phoneNumber, string inputOtp);

        /// <summary>
        /// Optionally: Clears the OTP after successful verification or expiry.
        /// </summary>
        Task InvalidateOtpAsync(string phoneNumber);
    }
}
