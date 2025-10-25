using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ClubIntf;
namespace Lumora.Web.Controllers.ClubAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.PostRoles)]
    public class PostLikeController(
        IPostLikeService postLikeService,
        ClubMessage messages,
        ILogger<PostLikeController> logger) : AuthenticatedController
    {
        private readonly IPostLikeService _postLikeService = postLikeService;
        private readonly ILogger<PostLikeController> _logger = logger;
        private readonly ClubMessage _messages = messages;

        /// <summary>
        /// Likes a post by the authenticated user.
        /// </summary>
        /// <param name="postId">ID of the post to like.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK if liked, 400 if already liked or invalid data, 401 if unauthorized, 404 if post not found, 500 if unexpected error.
        /// </returns>
        [HttpPost("like")]
        [RequiredPermission(Permissions.PostLikePermissions.Like)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Like([FromQuery] int postId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _postLikeService.LikeAsync(postId, CurrentUserId!, cancellationToken);
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
                _logger.LogError(ex, "PostLikeController - Like: Unexpected error while liking post.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Like post"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Unlikes a previously liked post by the authenticated user.
        /// </summary>
        /// <param name="postId">ID of the post to unlike.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK if unliked, 400 if not yet liked, 401 if unauthorized, 404 if post not found, 500 if unexpected error.
        /// </returns>
        [HttpPost("unlike")]
        [RequiredPermission(Permissions.PostLikePermissions.Unlike)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Unlike([FromQuery] int postId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _postLikeService.UnlikeAsync(postId, CurrentUserId!, cancellationToken);
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
                _logger.LogError(ex, "PostLikeController - Unlike: Unexpected error while unliking post.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Unlike post"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Checks if the authenticated user has liked a specific post.
        /// </summary>
        /// <param name="postId">ID of the post to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK with like status, 400 if user is invalid, 401 if unauthorized, 404 if post not found, 500 if unexpected error.
        /// </returns>
        [HttpGet("has-liked")]
        [RequiredPermission(Permissions.PostLikePermissions.HasLiked)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasLikedAsync([FromQuery] int postId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _postLikeService.HasLikedAsync(postId, CurrentUserId!, cancellationToken);
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
                _logger.LogError(ex, "PostLikeController - HasLiked: Unexpected error while checking like status.");
                return StatusCode(500, new GeneralResult<bool>(
                    false,
                    _messages.GetUnexpectedErrorMessage("Check like status"),
                    false,
                    ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Gets the total number of likes for a specific post.
        /// </summary>
        /// <param name="postId">ID of the post to retrieve like count for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK with like count, 404 if post not found, 500 if unexpected error.
        /// </returns>
        [HttpGet("count")]
        [RequiredPermission(Permissions.PostLikePermissions.GetLikeCount)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLikeCountAsync([FromQuery] int postId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _postLikeService.GetLikeCountAsync(postId, cancellationToken);
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
                _logger.LogError(ex, "PostLikeController - GetLikeCount: Unexpected error while retrieving like count for post {PostId}.", postId);
                return StatusCode(500, new GeneralResult<int>(
                    false,
                    _messages.GetUnexpectedErrorMessage("Get Like Count"),
                    0,
                    ErrorType.InternalServerError));
            }
        }
    }
}
