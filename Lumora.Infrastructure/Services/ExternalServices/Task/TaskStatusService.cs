namespace Lumora.Infrastructure.Services.TaskSvc
{
    public class TaskStatusService : ITaskStatusService
    {
        private readonly Dictionary<string, bool> taskStatusByName = new Dictionary<string, bool>();

        public void SetInitialState(string name, bool running)
        {
            if (!taskStatusByName.ContainsKey(name))
            {
                taskStatusByName[name] = running;
            }
        }

        public bool IsRunning(string name)
        {
            if (taskStatusByName.ContainsKey(name))
            {
                return taskStatusByName[name];
            }
            else
            {
                return false;
            }
        }

        public void SetRunning(string name, bool running)
        {
            taskStatusByName[name] = running;
        }
    }
}
