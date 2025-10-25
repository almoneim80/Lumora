using Lumora.DataAnnotations;
using Lumora.DTOs.Test;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.TestIntf;

namespace Lumora.Web.Controllers.TestAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class TestQuestionAdminController(
        ITestQuestionService service,
        TestMessage messages,
        ILogger<TestQuestionAdminController> logger) : AuthenticatedController
    {
        /// <summary>
        /// Creates a new question for a specific test with its choices.
        /// </summary>
        [HttpPost("create")]
        [RequiredPermission(Permissions.TestQuestionPermissions.Create)]
        [SwaggerOperation(Tags = new[] { "AdminTest" })]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] QuestionWithChoiseCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await service.AddQuestionAsync(dto, cancellationToken);
                if (!result.IsSuccess)
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
                logger.LogError(ex, "Create-Question - Unexpected error while creating test question.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("create question"), Data = null });
            }
        }

        /// <summary>
        /// Updates an existing question and its choices.
        /// </summary>
        [HttpPatch("update")]
        [RequiredPermission(Permissions.TestQuestionPermissions.Update)]
        [SwaggerOperation(Tags = new[] { "AdminTest" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromQuery] int questionId, [FromBody] DTOs.Test.TestQuestionUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(logger);
                if (modelCheck != null) return modelCheck;

                var result = await service.UpdateQuestionAsync(questionId, dto, cancellationToken);
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
                logger.LogError(ex, "Update-Question - Unexpected error while updating test question.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("update question"), Data = null });
            }
        }

        /// <summary>
        /// Deletes a question (soft delete) and all its related choices and answers.
        /// </summary>
        [HttpDelete("delete")]
        [RequiredPermission(Permissions.TestQuestionPermissions.Delete)]
        [SwaggerOperation(Tags = new[] { "AdminTest" })]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] int questionId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (questionId <= 0)
                    return BadRequest(new GeneralResult(false, messages.MsgIdInvalid));

                var result = await service.DeleteQuestionAsync(questionId, cancellationToken);
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
                logger.LogError(ex, "Delete-Question - Unexpected error while deleting test question.");
                return StatusCode(500, new GeneralResult(false, messages.GetUnexpectedErrorMessage("delete question")));
            }
        }

        /// <summary>
        /// Retrieves a specific question and its choices.
        /// </summary>
        [HttpGet("get")]
        [RequiredPermission(Permissions.TestQuestionPermissions.Get)]
        [SwaggerOperation(Tags = new[] { "AdminTest" })]
        [ProducesResponseType(typeof(GeneralResult<QuestionDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int questionId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (questionId <= 0)
                    return BadRequest(new GeneralResult(false, messages.MsgIdInvalid));

                var result = await service.GetQuestionByIdAsync(questionId, cancellationToken);
                if (!result.IsSuccess)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get-Question - Unexpected error while retrieving question {QuestionId}.", questionId);
                return StatusCode(500, new GeneralResult(false, messages.GetUnexpectedErrorMessage("get question")));
            }
        }

        /// <summary>
        /// Retrieves paginated questions for a specific test.
        /// </summary>
        [HttpGet("get-by-test")]
        [RequiredPermission(Permissions.TestQuestionPermissions.GetByTest)]
        [SwaggerOperation(Tags = new[] { "AdminTest" })]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<QuestionDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByTest([FromQuery] int testId, [FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (testId <= 0)
                    return BadRequest(new GeneralResult(false, messages.MsgTestIdInvalid));

                var result = await service.GetQuestionsByTestIdAsync(testId, pagination, cancellationToken);
                if (!result.IsSuccess)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get-Questions - Unexpected error while retrieving questions for test {TestId}.", testId);
                return StatusCode(500, new GeneralResult(false, messages.GetUnexpectedErrorMessage("get questions for test")));
            }
        }
    }
}
