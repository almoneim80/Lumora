using Lumora.DataAnnotations;
using Lumora.DTOs.Club;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.Club;
using Lumora.Interfaces.ClubIntf;
using Lumora.Services.Club;

namespace Lumora.Web.Controllers.ClubAPI.Admin
{
    [ApiController]
    [Route("wejha/[controller]")]
    //[Authorize(Roles = "Admin, SubAdmin, User")]
    public class PostAdminController(
        IPostLikeService postLikeService,
        IPostService postService,
        ClubMessage messages,
        ILogger<PostAdminController> logger) : AuthenticatedController
    {
        private readonly IPostLikeService _postLikeService = postLikeService;
        private readonly IPostService _postService = postService;
        private readonly ILogger<PostAdminController> _logger = logger;
        private readonly ClubMessage _messages = messages;

        /// <summary>
        /// Reviews a post by approving or rejecting it based on the specified status.
        /// </summary>
        /// <param name="dto">DTO containing the post ID and new status (Approved or Rejected).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult"/>:
        /// - 200 OK if the review is processed successfully.
        /// - 400 Bad Request for validation or processing errors.
        /// - 401 Unauthorized if the requester lacks authorization.
        /// - 404 Not Found if the post does not exist.
        /// - 500 Internal Server Error for unexpected errors.
        /// </returns>
        [HttpPatch("review")]
        [RequiredPermission(Permissions.PostAdminPermissions.ReviewPost)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReviewPost([FromForm] PostStatusUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _postService.ReviewPostAsync(dto, CurrentUserId!, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostController - ReviewPost: Unexpected error occurred while reviewing post.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Review post"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves a paginated list of all posts that are pending review.
        /// </summary>
        /// <param name="pagination">Pagination parameters such as page number and size.</param>
        /// <param name="cancellationToken">Token to handle cancellation requests.</param>
        [HttpGet("get-pending")]
        [RequiredPermission(Permissions.PostAdminPermissions.GetPendingPosts)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<PostDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendingPosts([FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _postService.GetPendingPostsAsync(pagination, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostController - GetPendingPosts: Unexpected error occurred while retrieving pending posts.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Get pending posts"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves all posts created by the specified user with pagination support.
        /// </summary>
        /// <param name="userId">ID of the user whose posts are to be fetched.</param>
        /// <param name="pagination">Pagination settings including page number and page size.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        [HttpGet("get-by-user")]
        [RequiredPermission(Permissions.PostAdminPermissions.GetUserPosts)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<PostDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllByUser([FromQuery] string userId, [FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new GeneralResult
                    {
                        IsSuccess = false,
                        Message = _messages.MsgUserIdRequired
                    });
                }

                var result = await _postService.GetAllByUserAsync(userId, pagination, true, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostController - GetAllByUser: Unexpected error occurred while retrieving user posts.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Get user posts"),
                    Data = null
                });
            }
        }
    }
}
