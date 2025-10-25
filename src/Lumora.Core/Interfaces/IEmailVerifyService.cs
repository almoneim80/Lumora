namespace Lumora.Interfaces;

public interface IEmailVerifyService
{
    Task<Domain> Verify(string email);
}
