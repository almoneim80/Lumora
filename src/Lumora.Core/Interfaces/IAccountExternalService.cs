namespace Lumora.Interfaces
{
    public interface IAccountExternalService
    {
        Task<AccountDetailsInfo?> GetAccountDetails(string domain);
    }
}
