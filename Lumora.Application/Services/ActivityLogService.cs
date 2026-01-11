namespace Lumora.Application.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _repository;
        public ActivityLogService(IActivityLogRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get max id for specific source.
        /// </summary>
        public async Task<int> GetMaxId(string source)
        {
            return await _repository.GetMaxIdAsync(source);
        }

        /// <summary>
        /// Add activity records to elastic search index as bulk.
        /// </summary>
        public async Task<bool> AddActivityRecords(List<ActivityLog> records)
        {
            if (records == null || !records.Any()) return true;
            return await _repository.AddRecordsAsync(records);
        }
    }
}
