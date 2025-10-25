namespace Lumora.Interfaces.UserIntf
{
    public interface IUserRepository
    {
        Task<bool> ExistsAsync(string userId);
    }
}
