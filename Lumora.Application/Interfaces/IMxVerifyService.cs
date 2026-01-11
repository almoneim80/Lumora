namespace Lumora.Application.Interfaces
{
    public interface IMxVerifyService
    {
        Task<bool> Verify(string mxValue);
    }
}
