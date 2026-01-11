namespace Lumora.Application.Interfaces
{
    public interface IEmailVerifyService
    {
        Task<WebDomain> Verify(string email);
    }
}
