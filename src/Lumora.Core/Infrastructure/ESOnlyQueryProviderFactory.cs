using System.Web;

namespace Lumora.Infrastructure
{
    public class ESOnlyQueryProviderFactory<T> : QueryProviderFactory<T>
        where T : BaseEntityWithId, new()
    {
        public ESOnlyQueryProviderFactory(PgDbContext dbContext, EsDbContext esDbContext, IOptions<Configuration.ApiSettingsConfig> apiSettingsConfig, IHttpContextHelper? httpContextHelper)
            : base(dbContext, esDbContext, apiSettingsConfig, httpContextHelper)
        {
        }

        public override IQueryProvider<T> BuildQueryProvider(int limit = -1)
        {
            var queryCommands = QueryStringParser.Parse(httpContextHelper.Request.QueryString.HasValue ? HttpUtility.UrlDecode(httpContextHelper.Request.QueryString.ToString()) : string.Empty);

            var queryBuilder = new QueryModelBuilder<T>(queryCommands, limit == -1 ? apiSettingsConfig.Value.MaxListSize : limit, dbContext);

            var indexPrefix = dbContext.Configuration.GetSection("Elastic:IndexPrefix").Get<string>();
            return new ESQueryProvider<T>(elasticClient, queryBuilder, indexPrefix!);
        }
    }
}
