namespace Lumora.Application.Interfaces
{
    public interface ILockService
    {
        ILockHolder Lock(string key);

        ILockHolder? TryLock(string key);
    }
}
