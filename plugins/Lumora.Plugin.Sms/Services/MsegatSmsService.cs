//using Serilog;
//using Lumora.Entities.Tables;

//namespace Lumora.Plugin.Sms.Services;

//public class MsegatSmsService : ISmsService
//{
//    private readonly MsegatConfig _msegatConfig;

//    public MsegatSmsService(MsegatConfig msegatConfig)
//    {
//        _msegatConfig = msegatConfig ?? throw new ArgumentNullException(nameof(msegatConfig));

//        try
//        {
//            // تحقق من صحة الإعدادات (مثلاً التأكد من وجود القيم الأساسية)
//            if (string.IsNullOrWhiteSpace(_msegatConfig.ApiUrl) ||
//                string.IsNullOrWhiteSpace(_msegatConfig.UserName) ||
//                string.IsNullOrWhiteSpace(_msegatConfig.ApiKey) ||
//                string.IsNullOrWhiteSpace(_msegatConfig.SenderId))
//            {
//                throw new MsegatException("Invalid Msegat configuration. Please check the API URL, UserName, ApiKey, and SenderId.");
//            }

//            // اختياري: إجراء طلب اختبار للتحقق من الاتصال بـ Msegat API
//            TestConnection().Wait();
//        }
//        catch (Exception ex)
//        {
//            Log.Error("Failed to initialize MsegatSmsService: {0}", ex.Message);
//        }
//    }

//    private async Task TestConnection()
//    {
//        using var httpClient = new HttpClient();
//        try
//        {
//            // مثال على طلب GET لاختبار الاتصال بـ API
//            var response = await httpClient.GetAsync(_msegatConfig.ApiUrl);
//            if (!response.IsSuccessStatusCode)
//            {
//                throw new MsegatException($"Failed to connect to Msegat API. StatusCode: {response.StatusCode}");
//            }

//            Log.Information("Successfully connected to Msegat API during initialization.");
//        }
//        catch (Exception ex)
//        {
//            Log.Error("Test connection to Msegat API failed: {0}", ex.Message);
//            throw;
//        }
//    }

//    public string GetSender(string recipient)
//    {
//        return _msegatConfig.SenderId;
//    }

//    public async Task SendAsync(string recipient, string message)
//    {
//        using var httpClient = new HttpClient();

//        try
//        {
//            // بناء المعلمات المطلوبة
//            var parameters = new Dictionary<string, string>
//        {
//            { "userName", _msegatConfig.UserName },
//            { "apiKey", _msegatConfig.ApiKey },
//            { "numbers", recipient },
//            { "userSender", _msegatConfig.SenderId },
//            { "msg", message },
//            { "msgEncoding", "UTF8" } // UTF8 لضمان الترميز الصحيح
//        };

//            // إرسال الطلب إلى API الخاص بـ Msegat
//            var content = new FormUrlEncodedContent(parameters);
//            var response = await httpClient.PostAsync(_msegatConfig.ApiUrl, content);

//            // التحقق من حالة الطلب
//            if (!response.IsSuccessStatusCode)
//            {
//                var responseContent = await response.Content.ReadAsStringAsync();
//                throw new SmsPluginException($"Failed to send SMS: {response.StatusCode} - {responseContent}");
//            }

//            Log.Information("SMS sent successfully to {Recipient} via Msegat gateway.", recipient);
//        }
//        catch (Exception ex)
//        {
//            Log.Error(ex, "Failed to send SMS to {Recipient} via Msegat gateway.", recipient);
//            throw new SmsPluginException($"Error sending SMS via Msegat: {ex.Message}", ex);
//        }
//    }

//    Task<AddSmsResult> ISmsService.AddSmsToDb(CreateSmsDto create)
//    {
//        throw new NotImplementedException();
//    }

//    Task<List<SmsLog>> ISmsService.GetAllSms()
//    {
//        throw new NotImplementedException();
//    }

//    Task<SmsLog> ISmsService.GetSmsById(int id)
//    {
//        throw new NotImplementedException();
//    }

//    string ISmsService.ValidateAndFormatPhoneNumber(string phoneNumber)
//    {
//        throw new NotImplementedException();
//    }

//    Task ISmsService.SuccessSent(int? smsLogId)
//    {
//        throw new NotImplementedException();
//    }

//    string ISmsService.ReplacePlaceholders(string templateContent, Dictionary<string, string> placeholders)
//    {
//        throw new NotImplementedException();
//    }
//}
