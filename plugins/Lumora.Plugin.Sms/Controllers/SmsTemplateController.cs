//using Microsoft.Extensions.Logging;
//namespace Lumora.Plugin.Sms.Controllers;

///// <summary>
///// sms template controller class that extends the base class.
///// </summary>
//[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
////[Authorize(Roles = "Admin, Trainer, Student")]
//public class SmsTemplateController : ControllerBase
//{
//    private readonly ILogger<SmsTemplateController> _logger;
//    public SmsTemplateController(
//        ILogger<SmsTemplateController> logger)
//    {
//        _logger = logger;
//        _baseService = service;
//    }

//    /// <summary>
//    /// Retrieves all sms templates from the database that are not marked as deleted.
//    /// </summary>
//    [HttpGet("GetAll")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult> GetAll()
//    {
//        try
//        {
//            var results = await _baseService.GetAllAsync();

//            if (results.Count == 0)
//            {
//                _logger.LogWarning("No results found in GetAll.");
//                return NotFound("No results found");
//            }

//            Response.Headers.Append("X-Total-Count", results.Count.ToString());
//            _baseService.RemoveSecondLevelObjects(results);

//            _logger.LogInformation("Successfully retrieved all records.");
//            return Ok(new
//            {
//                message = "Get All Success",
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
//    /// Retrieves a single sms template by its identifier.
//    /// </summary>
//    [HttpGet("GetOne/{id}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult> GetOne(int id)
//    {
//        try
//        {
//            var result = await _baseService.GetOneAsync(id);
//            if (result == null)
//            {
//                _logger.LogWarning("Record not found for ID: {Id}", id);
//                return NotFound("Record Not Found");
//            }

//            _logger.LogInformation("Successfully retrieved record with ID: {Id}", id);
//            return Ok(new
//            {
//                message = "Get One Success",
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
//    /// Creates a new sms template and saves it to the database.
//    /// </summary>
//    [HttpPost]
//    [ProducesResponseType(StatusCodes.Status201Created)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult> Create([FromBody] SmsTemplateCreateDto createDto)
//    {
//        try
//        {
//            var result = await _baseService.CreateAsync(createDto);
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
//    /// Updates an existing sms template partially by applying the provided data.
//    /// </summary>
//    [HttpPatch("{id}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public virtual async Task<ActionResult> Patch(int id, [FromBody] SmsTemplateUpdateDto updateDto)
//    {
//        try
//        {
//            var result = await _baseService.UpdateAsync(id, updateDto);
//            if (result == null)
//            {
//                _logger.LogWarning("Record not found for patch operation with ID: {Id}", id);
//                return NotFound("Record Not Found");
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
//    /// Soft deletes an existing sms template by setting its IsDeleted property to true.
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
//            var result = await _baseService.DeleteAsync(id);
//            if (!result)
//            {
//                _logger.LogWarning("Record not found for delete operation with ID: {Id}", id);
//                return NotFound("Record Not Found");
//            }

//            _logger.LogInformation("Successfully deleted record with ID: {Id}", id);
//            return Ok(new { Message = "Delete Success" });
//            }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "An error occurred in Delete for ID: {Id}", id);
//            throw;
//        }
//    }
//}
