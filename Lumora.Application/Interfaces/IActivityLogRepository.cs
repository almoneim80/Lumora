namespace Lumora.Application.Interfaces
{
    public interface IActivityLogRepository
    {
        Task<int> GetMaxIdAsync(string source);
        Task<bool> AddRecordsAsync(List<ActivityLog> records);
    }
}
