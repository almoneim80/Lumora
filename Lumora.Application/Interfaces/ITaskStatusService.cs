namespace Lumora.Application.Interfaces
{
    public interface ITaskStatusService
    {
        void SetInitialState(string name, bool running);
        bool IsRunning(string name);
        void SetRunning(string name, bool running);
    }
}
