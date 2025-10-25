using Microsoft.AspNetCore.Identity;
using Lumora.DataAnnotations;
using Lumora.DTOs.Club;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.Club;

namespace Lumora.Web.Controllers.ClubAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.PostRoles)]
    public class PostController(
        ILogger<PostController> logger,
        ClubMessage messages,
        FileValidatorHelper fileValidator,
        IPostService postService) : AuthenticatedController
    {
        private readonly ILogger<PostController> _logger = logger;
        private readonly ClubMessage _messages = messages;
        private readonly IPostService _postService = postService;
        private readonly FileValidatorHelper _fileValidator = fileValidator;

        /// <summary>
        /// Creates a new club post.
        /// </summary>
        /// <param name="formDto">The data required to create the post (content, media, etc).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK if created, 400 if invalid data, 401 if unauthorized, 500 if unexpected error.
        /// </returns>
        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [RequiredPermission(Permissions.PostPermissions.Create)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] PostCreateFormDto formDto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (string.IsNullOrWhiteSpace(formDto.Content) && formDto.MediaFile == null)
                    return BadRequest(new GeneralResult(false, _messages.MsgContentOrMediaRequired, null, ErrorType.Validation));

                string? mediaUrl = null;

                if (formDto.MediaFile != null)
                {
                    if (formDto.MediaType == null)
                        return BadRequest(new GeneralResult(false, _messages.MsgMediaTypeRequired, null, ErrorType.Validation));

                    var fileResult = formDto.MediaFile.PrepareValidatedFile(formDto.MediaType.Value, _fileValidator);
                    if (!fileResult.IsValid)
                        return BadRequest(new GeneralResult(false, fileResult.ErrorMessage!, null, ErrorType.Validation));

                    mediaUrl = fileResult.UniqueName;
                    // await _fileStorage.UploadAsync(...)
                }

                var dto = new PostCreateDto
                {
                    Content = formDto.Content,
                    MediaFile = mediaUrl,
                    MediaType = formDto.MediaType
                };


                var result = await _postService.CreateAsync(dto, CurrentUserId!, cancellationToken);
                if (result.IsSuccess == false)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostController - Create: Unexpected error while creating post.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Create post"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Deletes a post created by the current user.
        /// </summary>
        /// <param name="postId">ID of the post to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK if deleted, 400 if invalid input, 401 if unauthorized, 403 if not allowed, 404 if not found, 500 if internal error.
        /// </returns>
        [HttpDelete("delete")]
        [RequiredPermission(Permissions.PostPermissions.Delete)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] int postId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (postId <= 0)
                {
                    return BadRequest(new GeneralResult
                    {
                        IsSuccess = false,
                        Message = _messages.MsgPostIdInvalid,
                        Data = null
                    });
                }

                var result = await _postService.DeleteAsync(postId, CurrentUserId!, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.Forbidden => StatusCode(403, result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostController - Delete: Unexpected error while deleting post {PostId}.", postId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Delete post"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves the details of a specific post by its identifier.
        /// </summary>
        /// <param name="postId">The unique identifier of the post to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for request cancellation.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <see cref="GeneralResult{PostDetailsDto}"/>:
        /// - 200 OK if the post is retrieved successfully.
        /// - 400 Bad Request if the post ID is invalid.
        /// - 401 Unauthorized if the user is not authenticated.
        /// - 404 Not Found if the post is not found.
        /// - 500 Internal Server Error for unhandled exceptions.
        /// </returns>
        [HttpGet("get-one")]
        [RequiredPermission(Permissions.PostPermissions.GetById)]
        [ProducesResponseType(typeof(GeneralResult<PostDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery] int postId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (postId <= 0)
                {
                    return BadRequest(new GeneralResult
                    {
                        IsSuccess = false,
                        Message = _messages.MsgPostIdInvalid,
                        Data = null
                    });
                }

                var result = await _postService.GetByIdAsync(postId, cancellationToken);

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
                _logger.LogError(ex, "PostController - GetById: Unexpected error while retrieving post {PostId}.", postId);
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Get post"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves all public approved posts with pagination support.
        /// </summary>
        /// <param name="pagination">Pagination settings including page number and page size.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        [HttpGet("get-all-public")]
        [RequiredPermission(Permissions.PostPermissions.GetAllPublic)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<PostDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPublic([FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _postService.GetAllPublicAsync(pagination, cancellationToken);

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
                _logger.LogError(ex, "PostController - GetAllPublic: Unexpected error occurred while retrieving public posts.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = _messages.GetUnexpectedErrorMessage("Get public posts"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves all posts created by the specified user with pagination support.
        /// </summary>
        /// <param name="pagination">Pagination settings including page number and page size.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        [HttpGet("get-by-user")]
        [RequiredPermission(Permissions.PostPermissions.GetAllByUser)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<PostDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllByUser([FromQuery] PaginationRequestDto pagination, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _postService.GetAllByUserAsync(CurrentUserId!, pagination,  false, cancellationToken);

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
