using Serilog;
using Lumora.Services.BaseSvc;

namespace Lumora.Tasks
{
    public class HardDeleteTask(
        IConfiguration configuration,
        TaskStatusService taskStatusService,
        CascadeDeleteService deleteService) : BaseTask("Tasks:HardDeleteTask", configuration, taskStatusService)
    {
        private readonly CascadeDeleteService _deleteService = deleteService;

        public override async Task<bool> Execute(TaskExecutionLog currentJob)
        {
            try
            {
                await Task.Run(() => _deleteService.HardDeleteExpiredEntitiesAsync<User>());
                Log.Information($"Hard deleted expired entities. {currentJob.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"No expired entities found for hard delete. {currentJob.Id}");
                return false;
            }
        }
    }
}
