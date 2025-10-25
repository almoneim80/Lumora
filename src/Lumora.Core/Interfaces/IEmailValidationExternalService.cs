using Lumora.DTOs.Email;

namespace Lumora.Interfaces
{
    // واجهة لخدمة التحقق الخارجية من البريد الإلكتروني
    public interface IEmailValidationExternalService
    {
        Task<EmailVerifyInfoDto> Validate(string email);
    }
}
