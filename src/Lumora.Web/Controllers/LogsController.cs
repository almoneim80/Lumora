//namespace Lumora.Controllers;

//[Authorize(Roles = "Admin")]
//[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
//public class LogsController : Controller
//{
//    protected readonly EsDbContext esDbContext;

//    public LogsController(EsDbContext esDbContext)
//    {
//        this.esDbContext = esDbContext;
//    }

//    /// <summary>
//    /// Returns all logs.
//    /// </summary>
//    [HttpGet]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult<List<LogRecord>>> GetAll()
//    {
//        var logRecords = (
//                await esDbContext.ElasticClient.SearchAsync<LogRecord>(
//                    s => s.Size(20).Skip(10)))
//            .Documents.ToList();

//        return Ok(logRecords);
//    }

//    /// <summary>
//    /// Search logs.
//    /// </summary>
//    [HttpGet("search")]
//    public async Task<IActionResult> SearchLogs(string query)
//    {
//        var logs = await esDbContext.ElasticClient.SearchAsync<LogRecord>(s => s
//            .Query(q => q.Match(m => m.Field(f => f.Message).Query(query))));
//        return Ok(logs.Documents);
//    }
//}
