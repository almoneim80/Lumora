using Lumora.DataAnnotations;
using Lumora.DTOs.Test;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.TestIntf;

namespace Lumora.Web.Controllers.TestAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.TestRoles)]
    public class TestAttemptController(
        ITestAttemptService service,
        TestMessage messages,
        ILogger<TestAttemptController> logger) : AuthenticatedController
    {
        /// <summary>
        /// Starts a new test attempt for the specified test and authenticated user.
        /// </summary>
        [HttpPost("start")]
        [RequiredPermission(Permissions.TestAttemptPermissions.StartAttempt)]
        [SwaggerOperation(Tags = new[] { "Test" })]
        [ProducesResponseType(typeof(GeneralResult<TestAttemptStartDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartAttempt([FromQuery] int testId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await service.StartAttemptAsync(CurrentUserId!, testId, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while starting test attempt.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("start test attempt"), Data = null });
            }
        }

        /// <summary>
        /// Submits a user's answer for a test question within an attempt.
        /// </summary>
        [HttpPost("submit-answer")]
        [RequiredPermission(Permissions.TestAttemptPermissions.SubmitAnswer)]
        [SwaggerOperation(Tags = new[] { "Test" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitAnswer([FromBody] TestAnswerDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await service.SubmitAnswerAsync(dto, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while submitting answer.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("submit answer"), Data = null });
            }
        }

        /// <summary>
        /// Submits a completed test attempt for evaluation.
        /// </summary>
        [HttpPost("submit-attempt")]
        [RequiredPermission(Permissions.TestAttemptPermissions.SubmitAttempt)]
        [SwaggerOperation(Tags = new[] { "Test" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitAttempt([FromQuery] int attemptId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await service.SubmitAttemptAsync(attemptId, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while submitting test attempt.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("submit attempt"), Data = null });
            }
        }

        /// <summary>
        /// Retrieves the best attempt result for the specified test.
        /// </summary>
        [HttpGet("result")]
        [RequiredPermission(Permissions.TestAttemptPermissions.GetResult)]
        [SwaggerOperation(Tags = new[] { "Test" })]
        [ProducesResponseType(typeof(GeneralResult<TestAttemptResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetResult([FromQuery] int testId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await service.GetBestAttemptResultAsync(CurrentUserId!, testId, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while retrieving attempt result.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("get attempt result"), Data = null });
            }
        }

        /// <summary>
        /// Retrieves paginated list of test attempts made by the current user.
        /// </summary>
        [HttpGet("my-attempts")]
        [RequiredPermission(Permissions.TestAttemptPermissions.GetMyAttempts)]
        [SwaggerOperation(Tags = new[] { "Test" })]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<TestAttemptSummaryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyAttempts([FromQuery] int testId, [FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await service.GetUserAttemptsAsync(CurrentUserId!, testId, pagination, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while retrieving user's attempts.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("get my attempts"), Data = null });
            }
        }
    }
}
