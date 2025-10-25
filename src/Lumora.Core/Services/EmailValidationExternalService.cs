using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Serilog;
using Lumora.Configuration;
using Lumora.DTOs.Email;

// تنفيذ خدمة التحقق الخارجية من البريد الإلكتروني
namespace Lumora.Services
{
    public class EmailValidationExternalService : IEmailValidationExternalService
    {
        protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions();
        private readonly IOptions<EmailVerificationApiConfig> emailVerificationApiConfig;

        // المنشئ: حقن إعدادات API التحقق من البريد الإلكتروني
        public EmailValidationExternalService(IOptions<EmailVerificationApiConfig> emailVerificationApiConfig)
        {
            this.emailVerificationApiConfig = emailVerificationApiConfig;

            // تكوين خيارات تسلسل JSON إذا لم يتم تكوينها بالفعل
            if (SerializeOptions.PropertyNamingPolicy == null)
            {
                JsonHelper.Configure(SerializeOptions, JsonNamingConvention.CamelCase);
            }
        }

        // تنفيذ دالة التحقق من البريد الإلكتروني
        public async Task<EmailVerifyInfoDto> Validate(string email)
        {
            // الحصول على URL و API Key من الإعدادات
            var apiUrl = emailVerificationApiConfig.Value.Url;
            var apiKey = emailVerificationApiConfig.Value.ApiKey;

            // إنشاء عميل HTTP وإعداد الرأس
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // إعداد معلمات الاستعلام
            var queryParams = new Dictionary<string, string>
            {
                ["apiKey"] = apiKey,
                ["emailAddress"] = email,
            };

            // إرسال طلب GET إلى API
            var response = await client.GetAsync(QueryHelpers.AddQueryString(apiUrl, queryParams!));

            if (response.IsSuccessStatusCode)
            {
                // تحليل الاستجابة الناجحة
                var emailVerify = JsonSerializer.Deserialize<EmailVerifyInfoDto>(response.Content.ReadAsStringAsync().Result, SerializeOptions);

                Log.Information("Success of resolving {0}", emailVerify!.EmailAddress!);

                return emailVerify;
            }
            else
            {
                // رمي استثناء في حالة الفشل
                var responseContent = await response.Content.ReadAsStringAsync();

                throw new KeyNotFoundException(responseContent);
            }
        }
    }
}
