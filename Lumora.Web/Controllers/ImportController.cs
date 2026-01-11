//namespace Lumora.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class ImportController<T, TI> : ControllerBase
//    where T : SharedData
//    where TI : ISharedData
//{
//    protected readonly ImportService<T, TI> _service;
//    private readonly ILogger<ImportController<T, TI>> _logger;
//    public ImportController(ImportService<T, TI> service, ILogger<ImportController<T, TI>> logger)
//    {
//        _service = service;
//        _logger = logger;
//    }

//    /// <summary>
//    /// Imports a list of records into the system.
//    /// </summary>
//    [HttpPost]
//    [RequestSizeLimit(100 * 1024 * 1024)]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<ImportResult>> Import([FromBody] List<TI> importRecords)
//    {
//        var result = await _service.ImportAsync(importRecords);
//        return Ok(new { message = "Import Success.", data = result });
//    }

//    /// <summary>
//    /// Imports data from a file (CSV or Excel).
//    /// </summary>
//    [HttpPost("importFromFile")]
//    [RequestSizeLimit(100 * 1024 * 1024)] // file size = 100 MB
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<ImportResult>> ImportFromFile(IFormFile file)
//    {
//        var result = await _service.ImportFromFileAsync(file);
//        return Ok(new { message = "Import Success.", data = result });
//    }
//}
