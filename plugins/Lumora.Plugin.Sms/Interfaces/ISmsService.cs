namespace Lumora.Plugin.Sms.Interfaces
{
    public interface ISmsService
    {
        /// <summary>
        /// Send an SMS message to the selected recipient.
        /// </summary>
        Task<GeneralResult> SendAsync(string recipient, string message);

        /// <summary>
        /// Gets the sender ID used for the specified recipient.
        /// </summary>
        GeneralResult GetSender(string recipient);

        /// <summary>
        /// add sms to db.
        /// </summary>
        Task<GeneralResult> AddSmsToDb(CreateSmsDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// get all sms.
        /// </summary>
        Task<GeneralResult<List<SmsLog>>> GetAllSms(CancellationToken cancellationToken);

        /// <summary>
        /// get one sms by id.
        /// </summary>
        Task<GeneralResult<SmsLog>> GetSmsById(int id, CancellationToken cancellationToken);

        /// <summary>
        /// Validate and format a phone number using PhoneNumbers library.
        /// </summary>
        string ValidateAndFormatPhoneNumber(string phoneNumber);

        /// <summary>
        /// Mark the SMS as sent successfully.
        /// </summary>
        Task<GeneralResult> SuccessSent(int? smsLogId, CancellationToken cancellationToken);

        /// <summary>
        /// Replaces placeholders in the provided template content with the corresponding values from the placeholders dictionary.
        /// </summary>
        string ReplacePlaceholders(string templateContent, Dictionary<string, string> placeholders);
    }
}
