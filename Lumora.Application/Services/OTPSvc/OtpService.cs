//namespace Lumora.Services.OTP
//{
//    public class OtpService : IOtpService
//    {
//        private readonly ISmsService _smsService;
//        private readonly OtpOptions _options;
//        private readonly ILogger<OtpService> _logger;
//        private readonly ICacheService _cacheService;
//        public OtpService(
//            ISmsService smsService,
//            IOptions<OtpOptions> options,
//            ILogger<OtpService> logger,
//            ICacheService cacheService)
//        {
//            _smsService = smsService;
//            _options = options.Value;
//            _logger = logger;
//            _cacheService = cacheService;
//        }

//        /// <inheritdoc/>
//        public async Task<GeneralResult> GenerateAndSendOtpAsync(string userId, string phoneNumber)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(phoneNumber))
//                {
//                    _logger.LogError("User ID and phone number are required.");
//                    return new GeneralResult(false, "User ID and phone number are required.");
//                }

//                var rateLimitKey = $"otp_sent_recently_{userId}";
//                if (await _cacheService.ExistsAsync(rateLimitKey))
//                {
//                    _logger.LogWarning("Rate limit exceeded. Please try again later.");
//                    return new GeneralResult(false, "Rate limit exceeded. Please try again later.");
//                }

//                var otpCode = GenerateSecureOtp();
//                var expireAt = DateTimeOffset.UtcNow.AddMinutes(_options.ExpireInMinutes);
//                var timeToLive = expireAt - DateTimeOffset.UtcNow;

//                await _cacheService.SetAsync($"otp_{userId}", otpCode, timeToLive);
//                await _cacheService.SetAsync(rateLimitKey, "1", TimeSpan.FromSeconds(_options.ResendCooldownSeconds));

//                var message = _smsService.ReplacePlaceholders(
//                    _options.SmsTemplate,
//                    new Dictionary<string, string>
//                    {
//                        { "otp", otpCode },
//                        { "minutes", _options.ExpireInMinutes.ToString() }
//                    });

//                await _smsService.SendAsync(phoneNumber, message);
//                _logger.LogInformation("OTP sent successfully.");
//                return new GeneralResult(true, "OTP sent successfully.");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to generate or send OTP for {PhoneNumber}.", phoneNumber);
//                return new GeneralResult(false, "Failed to generate or send OTP. Please try again.");
//            }
//        }

//        /// <inheritdoc/>
//        public async Task<GeneralResult<bool>> VerifyOtpAsync(string userId, string inputOtp)
//        {
//            try
//            {
//                var cachedOtp = await _cacheService.GetAsync<string>($"otp_{userId}");
//                var rateLimitKey = $"otp_sent_recently_{userId}";
//                var failedAttemptsKey = $"otp_failed_{userId}";
//                if (cachedOtp == null)
//                {
//                    _logger.LogError("OTP not found for user {UserId}.", userId);
//                    return new GeneralResult<bool>(false, "OTP not found.");
//                }

//                if (!string.Equals(cachedOtp, inputOtp, StringComparison.Ordinal))
//                {
//                    var attempts = await _cacheService.GetAsync<int>(failedAttemptsKey);
//                    if (++attempts >= _options.MaxFailedAttempts)
//                    {
//                        await _cacheService.SetAsync(rateLimitKey, "1", TimeSpan.FromMinutes(_options.BlockDurationMinutes)); // Temporary block
//                        _logger.LogWarning("User" + userId + " exceeded max OTP attempts (" + _options.MaxFailedAttempts + "). Temporary lock activated.");
//                        return new GeneralResult<bool>(false, "Temporary lock activated. Please try again later.");
//                    }

//                    await _cacheService.SetAsync(failedAttemptsKey, attempts, TimeSpan.FromMinutes(_options.FailedAttemptWindowMinutes));
//                    _logger.LogError("Invalid OTP.");
//                    return new GeneralResult<bool>(false, "Invalid OTP.");
//                }

//                await InvalidateOtpAsync(userId);
//                await _cacheService.RemoveAsync(failedAttemptsKey);
//                _logger.LogInformation("OTP verified successfully for user {UserId}", userId);
//                return new GeneralResult<bool>(true, "OTP verified successfully.");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to verify OTP.");
//                return new GeneralResult<bool>(false, "Failed to verify OTP. Please try again.");
//            }
//        }

//        /// <inheritdoc/>
//        public async Task<GeneralResult> ResendOtpAsync(string userId, string phoneNumber)
//        {
//            try
//            {
//                _logger.LogInformation("Resending OTP for " + phoneNumber + ".");
//                return await GenerateAndSendOtpAsync(userId, phoneNumber);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to resend OTP.");
//                return new GeneralResult(false, "Failed to resend OTP. Please try again.");
//            }
//        }

//        /// <inheritdoc/>
//        public async Task InvalidateOtpAsync(string userId)
//        {
//            _logger.LogInformation("Invalidating OTP for " + userId + ".");
//            await _cacheService.RemoveAsync($"otp_{userId}");
//        }

//        // private methods
//        private static string GenerateSecureOtp()
//        {
//            using var rng = RandomNumberGenerator.Create();
//            var bytes = new byte[4];
//            rng.GetBytes(bytes);
//            var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
//            return number.ToString("D6");
//        }
//    }
//}
