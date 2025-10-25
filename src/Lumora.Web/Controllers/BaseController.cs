//namespace Lumora.Controllers;

//[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
//public class BaseController<T, TC, TU, TD, TE> : ControllerBase
//    where T : SharedData, new()
//    where TC : class
//    where TU : class
//    where TD : class
//    where TE : class
//{
//    protected readonly BaseService<T, TC, TU, TD> _service;
//    private readonly ILocalizationManager? _localization;
//    private readonly ILogger<BaseController<T, TC, TU, TD, TE>> _logger;

//    public BaseController(BaseService<T, TC, TU, TD> service, ILocalizationManager? localization, ILogger<BaseController<T, TC, TU, TD, TE>> logger)
//    {
//        _service = service;
//        _localization = localization;
//        _logger = logger;
//    }

//    /// <summary>
//    /// Retrieves all entities from the database that are not marked as deleted.
//    /// </summary>
//    [HttpGet("GetAll")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult<TD[]>> GetAll()
//    {
//        try
//        {
//            var results = await _service.GetAllAsync();

//            if (results.Count == 0)
//            {
//                _logger.LogWarning("No results found in GetAll.");
//                return NotFound("No results found");
//            }

//            Response.Headers.Append("X-Total-Count", results.Count.ToString());
//            _service.RemoveSecondLevelObjects(results);

//            _logger.LogInformation("Successfully retrieved all records.");
//            return Ok(new
//            {
//                message = _localization!.GetLocalizedString("GetAllSuccess"),
//                data = results,
//            });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred in GetAll.");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Retrieves a single entity by its identifier.
//    /// </summary>
//    [HttpGet("GetOne/{id}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult<TD>> GetOne(int id)
//    {
//        try
//        {
//            var result = await _service.GetOneAsync(id);
//            if (result == null)
//            {
//                _logger.LogWarning("Record not found for ID: {Id}", id);
//                return NotFound(_localization!.GetLocalizedString("RecordNotFound"));
//            }

//            _logger.LogInformation("Successfully retrieved record with ID: {Id}", id);
//            return Ok(new
//            {
//                message = _localization!.GetLocalizedString("GetOneSuccess"),
//                date = result,
//            });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred in GetOne for ID: {Id}", id);
//            throw;
//        }
//    }

//    /// <summary>
//    /// Creates a new entity and saves it to the database.
//    /// </summary>
//    [HttpPost]
//    [ProducesResponseType(StatusCodes.Status201Created)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult<TD>> Create([FromBody] TC createDto)
//    {
//        try
//        {
//            var result = await _service.CreateAsync(createDto);
//            if (result == null)
//            {
//                _logger.LogWarning("Failed to create record.");
//                return BadRequest();
//            }

//            var id = ((dynamic)result).Id;
//            _logger.LogInformation("Successfully created record");
//            return CreatedAtAction(nameof(GetOne), new { id = id }, result);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred in Create.");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Updates an existing entity partially by applying the provided data.
//    /// </summary>
//    [HttpPatch("{id}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult<TD>> Patch(int id, [FromBody] TU updateDto)
//    {
//        try
//        {
//            var result = await _service.UpdateAsync(id, updateDto);
//            if (result == null)
//            {
//                _logger.LogWarning("Record not found for patch operation with ID: {Id}", id);
//                return NotFound(_localization!.GetLocalizedString("RecordNotFound"));
//            }

//            _logger.LogInformation("Successfully updated record with ID: {Id}", id);
//            return Ok(new { message = "Update Success.", data = result });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred in Patch for ID: {Id}", id);
//            throw;
//        }
//    }

//    /// <summary>
//    /// Soft deletes an existing entity by setting its IsDeleted property to true.
//    /// </summary>
//    [HttpDelete("{id}")]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult> Delete(int id)
//    {
//        try
//        {
//            var result = await _service.DeleteAsync(id);
//            if (!result)
//            {
//                _logger.LogWarning("Record not found for delete operation with ID: {Id}", id);
//                return NotFound(_localization!.GetLocalizedString("RecordNotFound"));
//            }

//            _logger.LogInformation("Successfully deleted record with ID: {Id}", id);
//            return Ok(new { Message = _localization!.GetLocalizedString("DeleteSuccess") });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred in Delete for ID: {Id}", id);
//            throw;
//        }
//    }

//    /// <summary>
//    /// Exports data to CSV.
//    /// </summary>
//    [HttpGet("CSVExport")]
//    [Produces("text/csv")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<IActionResult> ExportToCsv()
//    {
//        try
//        {
//            var csvData = await _service.ExportToCsvAsync<TE>();
//            if (string.IsNullOrEmpty(csvData))
//            {
//                _logger.LogWarning("No data available for CSV export.");
//                return NotFound(_localization!.GetLocalizedString("NoDataAvailable"));
//            }

//            var fileName = $"{typeof(T).Name}_Export_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
//            _logger.LogInformation("CSV export successful.");
//            return File(new System.Text.UTF8Encoding().GetBytes(csvData), "text/csv", fileName);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred during CSV export.");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Exports data to Excel.
//    /// </summary>
//    [HttpGet("ExcelExport")]
//    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<IActionResult> ExportToExcel()
//    {
//        try
//        {
//            var excelData = await _service.ExportToExcelAsync<TE>();

//            if (excelData == null || excelData.Length == 0)
//            {
//                _logger.LogWarning("No data available for Excel export.");
//                return NotFound(_localization!.GetLocalizedString("NoDataAvailable"));
//            }

//            var fileName = $"{typeof(T).Name}_Export_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
//            _logger.LogInformation("Excel export successful.");
//            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred during Excel export.");
//            throw;
//        }
//    }

//    /// <summary>
//    /// Export data to JSON format.
//    /// </summary>
//    [HttpGet("JSONExport")]
//    [Produces("application/json")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<IActionResult> ExportToJson()
//    {
//        try
//        {
//            var jsonData = await _service.ExportToJsonAsync<TE>();

//            if (string.IsNullOrEmpty(jsonData))
//            {
//                _logger.LogWarning("No data available for JSON export.");
//                return NotFound(_localization!.GetLocalizedString("NoDataAvailable"));
//            }

//            var fileName = $"{typeof(T).Name}_Export_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
//            _logger.LogInformation("JSON export successful.");
//            return File(new System.Text.UTF8Encoding().GetBytes(jsonData), "application/json", fileName);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred during JSON export.");
//            throw;
//        }
//    }
//}
