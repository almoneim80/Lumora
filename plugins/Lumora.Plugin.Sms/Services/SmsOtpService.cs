using System.Security.Cryptography;

namespace Lumora.Plugin.Sms.Services
{
    public class SmsOtpService(
        ISmsService smsService,
        IOptions<OtpOptions> options,
        ILogger<SmsOtpService> logger,
        ICacheService cacheService) : IOtpService
    {
        private readonly ISmsService _smsService = smsService;
        private readonly IOptions<OtpOptions> _options = options;
        private readonly ILogger<SmsOtpService> _logger = logger;
        private readonly ICacheService _cacheService = cacheService;

        /// <inheritdoc/>
        public async Task<GeneralResult> GenerateAndSendOtpAsync(string phoneNumber)
        {
            try
            {
                var message = await PrepareSmsMessage(phoneNumber, _options.Value.SmsTemplate);

                // Send SMS
                await _smsService.SendAsync(phoneNumber, message);

                _logger.LogInformation("OTP sent successfully to {PhoneNumber}, expires at {ExpireAt}", phoneNumber, _options.Value.ExpireInMinutes);
                return new GeneralResult(true, "OTP sent successfully.", null, Enums.ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate or send OTP for {PhoneNumber}.", phoneNumber);
                return new GeneralResult(false, "Failed to generate or send OTP.", null, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> ResendOtpAsync(string phoneNumber)
        {
            try
            {
                var message = await PrepareSmsMessage(phoneNumber, _options.Value.SmsTemplate);

                // Send SMS
                await _smsService.SendAsync(phoneNumber, message);

                _logger.LogInformation("OTP resent successfully to {PhoneNumber}, expires at {ExpireAt}", phoneNumber, _options.Value.ExpireInMinutes);
                return new GeneralResult(true, "OTP re-sent successfully.", null, Enums.ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend OTP for {PhoneNumber}.", phoneNumber);
                return new GeneralResult(false, "Feiled to resend OTP.", null, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<bool>> VerifyOtpAsync(string phoneNumber, string inputOtp)
        {
            try
            {
                var cacheKey = $"otp_{phoneNumber}";
                if(string.IsNullOrWhiteSpace(inputOtp))
                {
                    _logger.LogWarning("Invalid OTP for {PhoneNumber}.", phoneNumber);
                    return new GeneralResult<bool>(false, "Invalid OTP.", false, Enums.ErrorType.BadRequest);
                }

                var storedOtp = await _cacheService.GetAsync<string>(cacheKey);
                if (storedOtp == null)
                {
                    _logger.LogWarning("OTP not found for {PhoneNumber}.", phoneNumber);
                    return new GeneralResult<bool>(false, "OTP expired or not found.", false, Enums.ErrorType.BadRequest);
                }

                if (storedOtp != inputOtp)
                {
                    _logger.LogWarning("Invalid OTP for {PhoneNumber}.", phoneNumber);
                    return new GeneralResult<bool>(false, "Invalid OTP.", false, Enums.ErrorType.BadRequest);
                }

                _logger.LogInformation("OTP verified successfully for {PhoneNumber}.", phoneNumber);
                
                return new GeneralResult<bool>(true, "OTP verified successfully.", true, Enums.ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify OTP for {PhoneNumber}.", phoneNumber);
                await InvalidateOtpAsync(phoneNumber);
                return new GeneralResult<bool>(false, "Feailed to verify OTP.", false, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task InvalidateOtpAsync(string phoneNumber)
        {
            var cacheKey = $"otp_{phoneNumber}";
            await _cacheService.RemoveAsync(cacheKey);
        }

        #region Private Methods

        /// <summary>
        /// Generate OTP code.
        /// </summary>
        private string GenerateOtp()
        {
            // Generate OTP
            int code = RandomNumberGenerator.GetInt32(100000, 1000000);
            return code.ToString("D6");
        }

        /// <summary>
        /// Prepare SMS message.
        /// </summary>
        private async Task<string> PrepareSmsMessage(string phoneNumber, string template)
        {
            var now = DateTimeOffset.UtcNow;
            var otpCode = GenerateOtp();
            var expireAt = now.AddMinutes(_options.Value.ExpireInMinutes);

            // Save to cache
            var timeToLive = expireAt - now;
            await _cacheService.SetAsync($"otp_{phoneNumber}", otpCode, timeToLive);

            // Prepare SMS message
            var placeholders = new Dictionary<string, string>
                {
                    { "otp", otpCode },
                    { "minutes", _options.Value.ExpireInMinutes.ToString() }
                };

            var message = _smsService.ReplacePlaceholders(template, placeholders);

            return message;
        }

        #endregion
    }
}
