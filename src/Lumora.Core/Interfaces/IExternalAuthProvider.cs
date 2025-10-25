using Lumora.DTOs.Token;

namespace Lumora.Interfaces;

public interface IExternalAuthProvider
{
    Task<AuthResult> AuthenticateAsync(string token);
}
