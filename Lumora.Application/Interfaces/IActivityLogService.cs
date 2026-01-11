namespace Lumora.Application.Interfaces
{
    public interface IActivityLogService
    {
        Task<int> GetMaxId(string source);
        Task<bool> AddActivityRecords(List<ActivityLog> records);
    }
}
