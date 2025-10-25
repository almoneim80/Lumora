using Serilog;
using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Tasks
{
    public class SyncProgramProgressTask(
        IConfiguration configuration,
        TaskStatusService taskStatusService,
        IProgressService progressService) : BaseTask("Tasks:SyncProgramProgressTask", configuration, taskStatusService)
    {
        private readonly IProgressService _progressService = progressService;
        private readonly IConfiguration _configuration = configuration;

        public override async Task<bool> Execute(TaskExecutionLog currentJob)
        {
            try
            {
                var programId = _configuration.GetValue<int>("Tasks:SyncProgramProgressTask:ProgramId");

                var result = await _progressService.SyncAllUserProgressForProgramAsync(programId, CancellationToken.None);

                Log.Information($"Program progress sync completed for ProgramId={programId}. TaskLogId={currentJob.Id}. Success={result.IsSuccess}");
                return result.IsSuccess ?? false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error syncing program progress. TaskLogId={currentJob.Id}");
                return false;
            }
        }
    }   
}
