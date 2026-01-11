namespace Lumora.Infrastructure.Repositories
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly string _indexName;
        private readonly EsDbContext _esDbContext;
        public ActivityLogRepository(IConfiguration configuration, EsDbContext esDbContext)
        {
            _indexName = configuration["Elastic:IndexPrefix"] + "-activitylog";
            _esDbContext = esDbContext;
        }

        public async Task<int> GetMaxIdAsync(string source)
        {
            var sr = new SearchRequest<ActivityLog>(_indexName);
            sr.Query = new TermQuery() { Field = "source.keyword", Value = source };
            sr.Sort = new List<ISort>() { new FieldSort { Field = "sourceId", Order = Nest.SortOrder.Descending } };
            sr.Size = 1;

            var res = await _esDbContext.ElasticClient.SearchAsync<ActivityLog>(sr);

            if (res.IsValid && res.Documents.Any())
            {
                return res.Documents.First().SourceId;
            }

            return 0;
        }

        public async Task<bool> AddRecordsAsync(List<ActivityLog> records)
        {
            if (records == null || !records.Any()) return true;

            var response = await _esDbContext.ElasticClient.IndexManyAsync(records, _indexName);

            if (!response.IsValid)
            {
                Log.Error("Cannot save logs in Elastic Search. Reason: {Reason}", response.DebugInformation);
            }

            return response.IsValid;
        }
    }
}
