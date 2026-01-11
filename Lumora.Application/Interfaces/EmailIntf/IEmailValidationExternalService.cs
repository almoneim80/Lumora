namespace Lumora.Application.Interfaces
{
    // واجهة لخدمة التحقق الخارجية من البريد الإلكتروني
    public interface IEmailValidationExternalService
    {
        Task<EmailVerifyInfoDto> Validate(string email);
    }
}
