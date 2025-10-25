
namespace Lumora.Plugin.Sms.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly TwilioConfig twilioConfig;
        public TwilioSmsService(TwilioConfig twilioCfg)
        {
            twilioConfig = twilioCfg;
            try
            {
                TwilioClient.Init(twilioCfg.AccountSid, twilioCfg.AuthToken);
            }
            catch (ApiException e)
            {
                Log.Error("Failed to init twillio client {0}", e.Message);
            }
        }

        public string GetSender()
        {
            return twilioConfig.FromNumber;
        }

        public async Task SendAsync(string recipient, string message)
        {
            var options = new CreateMessageOptions(new Twilio.Types.PhoneNumber(recipient))
            {
                From = new Twilio.Types.PhoneNumber(twilioConfig.FromNumber),
                Body = message,
            };

            await MessageResource.CreateAsync(options);
            Log.Information("Sms message sent to {0} via Twilio gateway: {1}", recipient, message);
        }

        Task<GeneralResult> ISmsService.AddSmsToDb(CreateSmsDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<GeneralResult<List<SmsLog>>> ISmsService.GetAllSms(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        GeneralResult ISmsService.GetSender(string recipient)
        {
            throw new NotImplementedException();
        }

        Task<GeneralResult<SmsLog>> ISmsService.GetSmsById(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        string ISmsService.ReplacePlaceholders(string templateContent, Dictionary<string, string> placeholders)
        {
            throw new NotImplementedException();
        }

        Task<GeneralResult> ISmsService.SendAsync(string recipient, string message)
        {
            throw new NotImplementedException();
        }

        Task<GeneralResult> ISmsService.SuccessSent(int? smsLogId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        string ISmsService.ValidateAndFormatPhoneNumber(string phoneNumber)
        {
            throw new NotImplementedException();
        }
    }
}
