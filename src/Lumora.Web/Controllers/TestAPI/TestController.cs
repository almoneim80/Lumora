using Lumora.DataAnnotations;
using Lumora.DTOs.Test;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.TestIntf;

namespace Lumora.Web.Controllers.TestAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.TestRoles)]
    public class TestController(
            TestMessage messages,
            ITestService testService,
            ILogger<TestController> logger) : AuthenticatedController
    {
        /// <summary>
        /// Retrieves test by its ID.
        /// </summary>
        [HttpGet("get-one")]
        [RequiredPermission(Permissions.TestPermissions.GetTestById)]
        [ProducesResponseType(typeof(GeneralResult<TestDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTestById([FromQuery] int testId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (testId <= 0)
                    return BadRequest(new GeneralResult { IsSuccess = false, Message = messages.MsgIdInvalid });

                var result = await testService.GetTestByIdAsync(testId, cancellationToken);
                return result.IsSuccess ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving test by ID.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" get test by ID"), Data = null });
            }
        }
    }
}
