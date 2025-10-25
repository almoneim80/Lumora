using Lumora.DTOs.Test;
using Lumora.Interfaces.TestIntf;

namespace Lumora.Web.Controllers.TestAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class TestAdminController(
        TestMessage messages,
        ITestService testService,
        ILogger<TestQuestionAdminController> logger) : AuthenticatedController
    {
        // =============== TEST ===============

        /// <summary>
        /// Creates a new test with questions and choices.
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTest([FromBody] TestCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await testService.CreateTestAsync(dto, cancellationToken);
                return result.IsSuccess ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating test.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" create test"), Data = null });
            }
        }

        /// <summary>
        /// Updates an existing test.
        /// </summary>
        [HttpPatch("update")]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTest([FromQuery] int testId, [FromBody] TestUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await testService.UpdateTestAsync(testId, dto, cancellationToken);
                return result.IsSuccess == true ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating test.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" update test"), Data = null });
            }
        }

        /// <summary>
        /// Deletes a test by ID.
        /// </summary>
        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTest([FromQuery] int testId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (testId <= 0)
                    return BadRequest(new GeneralResult { IsSuccess = false, Message = messages.MsgIdInvalid });

                var result = await testService.DeleteTestAsync(testId, cancellationToken);
                return result.IsSuccess == true ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting test.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" delete test"), Data = null });
            }
        }

        /// <summary>
        /// Retrieves test by its ID.
        /// </summary>
        [HttpGet("get")]
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

        /// <summary>
        /// Retrieves tests for a specific lesson with pagination.
        /// </summary>
        [HttpGet("get-all")]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<TestDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTestsForLesson([FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await testService.GetAllTestsAsync(pagination, cancellationToken);
                return result.IsSuccess ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving tests for lesson.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" get tests for lesson"), Data = null });
            }
        }
    }
}
