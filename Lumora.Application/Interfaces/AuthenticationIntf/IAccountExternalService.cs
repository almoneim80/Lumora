namespace Lumora.Application.Interfaces
{
    public interface IAccountExternalService
    {
        Task<AccountDetailsInfo?> GetAccountDetails(string domain);
    }
}
