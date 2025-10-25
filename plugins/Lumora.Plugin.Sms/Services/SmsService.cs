namespace Lumora.Plugin.Sms.Services
{
    public class SmsService : ISmsService
    {
        private readonly PluginConfig _pluginSettings = new PluginConfig();
        private readonly Dictionary<string, ISmsService> _countrySmsServices = new Dictionary<string, ISmsService>();
        private readonly PgDbContext _dbContext;
        private readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();
        private readonly ILogger<SmsService> _logger;
        public SmsService(IConfiguration configuration, PgDbContext dbContext, ILogger<SmsService> logger)
        {
            var settings = configuration.Get<PluginConfig>();
            if (settings != null)
            {
                _pluginSettings = settings;
                InitGateways();
            }
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SendAsync(string recipient, string message)
        {
            try
            {
                var smsService = GetSmsService(recipient);
                if (smsService == null)
                {
                    return new GeneralResult(false, "SMS service not found.", null, Enums.ErrorType.BadRequest);
                }

                return await smsService.SendAsync(recipient, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS.");
                return new GeneralResult(false, "Failed to send SMS.", null, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public GeneralResult GetSender(string recipient)
        {
            try
            {
                var smsService = GetSmsService(recipient);
                if (smsService == null)
                {
                    _logger.LogWarning("SMS service not found.");
                    return new GeneralResult(false, "SMS service not found.", null, Enums.ErrorType.BadRequest);
                }

                return new GeneralResult(true, "Sender found.", smsService.GetSender(recipient), Enums.ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get sender.");
                return new GeneralResult(false, "Failed to get sender.", null, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> AddSmsToDb(CreateSmsDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogError("Create Sms Dto is null.");
                    return new GeneralResult(false, "all fields are required", null, Enums.ErrorType.BadRequest);
                }

                var smsLog = new SmsLog
                {
                    Sender = dto.Sender ?? string.Empty,
                    Recipient = dto.Recipient ?? string.Empty,
                    Message = dto.Message ?? string.Empty,
                    Status = SmsSendStatus.NotSent,
                    CreatedAt = DateTime.UtcNow,
                    Source = "Up",
                    IsDeleted = false,
                };

                _dbContext.SmsLogs.Add(smsLog);
                var result = await _dbContext.SaveChangesAsync(cancellationToken);
                if (result == 0)
                {
                    _logger.LogError("Failed to add SMS to database.");
                    return new GeneralResult(false, "Failed to add SMS to database.", null, Enums.ErrorType.InternalServerError);
                }
                else
                {
                    _logger.LogInformation("SMS added to database successfully.");
                    return new GeneralResult(true, "SMS added to database successfully.", null, Enums.ErrorType.Success);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add SMS to database.");
                return new GeneralResult(false, "Failed to add SMS to database.", null, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<SmsLog>>> GetAllSms(CancellationToken cancellationToken)
        {
            try
            {
                var smsLogs = await _dbContext.SmsLogs.AsNoTracking().Where(s => !s.IsDeleted).ToListAsync(cancellationToken);
                if (!smsLogs.Any())
                {
                    _logger.LogWarning("No SMS found.");
                    return new GeneralResult<List<SmsLog>>(false, "No SMS found.", null, Enums.ErrorType.NotFound);
                }

                _logger.LogInformation("All SMS found.");
                return new GeneralResult<List<SmsLog>>(true, "All SMS found.", smsLogs, Enums.ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all SMS.");
                return new GeneralResult<List<SmsLog>>(false, "Failed to get all SMS.", null, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<SmsLog>> GetSmsById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var smsLog = await _dbContext.SmsLogs.Where(s => s.Id == id && !s.IsDeleted).FirstOrDefaultAsync(cancellationToken);
                if (smsLog == null)
                {
                    _logger.LogWarning("SMS not found.");
                    return new GeneralResult<SmsLog>(false, "SMS not found.", null, Enums.ErrorType.NotFound);
                }

                _logger.LogInformation("SMS found.");
                return new GeneralResult<SmsLog>(true, "SMS found.", smsLog, Enums.ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get SMS.");
                return new GeneralResult<SmsLog>(false, "Failed to get SMS.", null, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> SuccessSent(int? smsLogId, CancellationToken cancellationToken)
        {
            try
            {
                if (smsLogId == null)
                {
                    _logger.LogWarning("SMS log ID is required.");
                    return new GeneralResult(false, "SMS log ID is required.", null, Enums.ErrorType.BadRequest);
                }

                var smsLog = await _dbContext.SmsLogs.FirstOrDefaultAsync(s => s.Id == smsLogId && !s.IsDeleted, cancellationToken);
                if (smsLog == null)
                {
                    _logger.LogWarning("SMS log not found.");
                    return new GeneralResult(false, "SMS log not found.", null, Enums.ErrorType.NotFound);
                }

                smsLog.Status = SmsSendStatus.Sent;
                var result = await _dbContext.SaveChangesAsync(cancellationToken);
                if (result == 0)
                {
                    _logger.LogError("Failed to update SMS status.");
                    return new GeneralResult(false, "Failed to update SMS status.", null, Enums.ErrorType.InternalServerError);
                }

                _logger.LogInformation("SMS status updated successfully.");
                return new GeneralResult(true, "SMS status updated successfully.", null, Enums.ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update SMS status.");
                return new GeneralResult(false, "Failed to update SMS status.", null, Enums.ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public string ValidateAndFormatPhoneNumber(string phoneNumber)
        {
            var parsedNumber = _phoneNumberUtil.Parse(phoneNumber, string.Empty);
            return _phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.E164);
        }

        /// <inheritdoc/>
        public string ReplacePlaceholders(string templateContent, Dictionary<string, string> placeholders)
        {
            foreach (var placeholder in placeholders)
            {
                templateContent = templateContent.Replace($"{{{placeholder.Key}}}", placeholder.Value);
            }
            return templateContent;
        }

        #region Private methods

        /// <summary>
        /// Get SMS service based on recipient's country code.
        /// </summary>
        private ISmsService? GetSmsService(string recipient)
        {
            var key = _countrySmsServices.Keys.FirstOrDefault(key => recipient.StartsWith(key));
            if (key != null)
            {
                return _countrySmsServices[key];
            }

            if (_countrySmsServices.TryGetValue("default", out var smsService))
            {
                return smsService;
            }

            return null;
        }

        /// <summary>
        /// Initialize SMS gateways.
        /// </summary>
        private void InitGateways()
        {
            foreach (var countryGateway in _pluginSettings.SmsCountryGateways)
            {
                ISmsService? gatewayService = null;

                var gatewayName = countryGateway.Gateway;

                switch (gatewayName)
                {
                    //case "Msegat":
                    //    gatewayService = new MsegatSmsService(_pluginSettings.SmsGateways.Msegat);
                    //    break;
                    case "Twilio":
                        gatewayService = new TwilioSmsService(_pluginSettings.SmsGateways.Twilio);
                        break;
                }

                if (gatewayService != null)
                {
                    _countrySmsServices[countryGateway.Code] = gatewayService;
                }
            }
        }

        #endregion
    }
}
