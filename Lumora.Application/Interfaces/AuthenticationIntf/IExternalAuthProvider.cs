namespace Lumora.Domain.Interfaces
{
    public interface IExternalAuthProvider
    {
        Task<AuthResult> AuthenticateAsync(string token);
    }
}
