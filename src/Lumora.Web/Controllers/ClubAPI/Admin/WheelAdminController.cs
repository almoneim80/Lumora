using Lumora.DataAnnotations;
using Lumora.DTOs.Club;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.ClubIntf;

namespace Lumora.Web.Controllers.ClubAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class WheelAdminController(
        WheelMessag messages,
        ILocalizationManager localization,
        IExtendedBaseService extendedBaseService,
        IWheelAwardService wheelAwardService,
        ILogger<WheelAdminController> logger,
        IWheelPlayerService wheelPlayerService) : AuthenticatedController
    {
        private readonly ILogger<WheelAdminController> _logger = logger;
        private readonly IWheelPlayerService _wheelPlayerService = wheelPlayerService;

        /// <summary>
        /// Creates a new wheel award.
        /// </summary>
        [HttpPost("create")]
        [RequiredPermission(Permissions.WheelAdminPermissions.CreateAward)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] WheelAwardCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await wheelAwardService.CreateAsync(dto, cancellationToken);
                return result.IsSuccess == true ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wheel award.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" create wheel award") });
            }
        }

        /// <summary>
        /// Updates an existing wheel award.
        /// </summary>
        [HttpPut("update")]
        [RequiredPermission(Permissions.WheelAdminPermissions.UpdateAward)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] WheelAwardUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await wheelAwardService.UpdateAsync(id, dto, cancellationToken);
                return result.IsSuccess == true ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wheel award.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" update wheel award") });
            }
        }

        /// <summary>
        /// Retrieves a specific wheel award by its unique identifier.
        /// </summary>
        [HttpGet("get-one")]
        [RequiredPermission(Permissions.WheelAdminPermissions.GetAwardById)]
        [ProducesResponseType(typeof(GeneralResult<WheelAwardDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery] int id, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                if (id <= 0)
                    return BadRequest(new GeneralResult { IsSuccess = false, Message = messages.MsgIdInvalid });

                var result = await wheelAwardService.GetByIdAsync(id, cancellationToken);
                return result.IsSuccess ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving wheel award by ID.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage("get wheel award by ID"), Data = null });
            }
        }

        /// <summary>
        /// Deletes a wheel award.
        /// </summary>
        [HttpDelete("delete")]
        [RequiredPermission(Permissions.WheelAdminPermissions.DeleteAward)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromQuery] int id, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await wheelAwardService.DeleteAsync(id, cancellationToken);
                return result.IsSuccess == true ? Ok(result) : result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting wheel award.");
                return StatusCode(500, new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" delete wheel award") });
            }
        }

        /// <summary>
        /// Gets all award types.
        /// </summary>
        [HttpGet("award-type")]
        [RequiredPermission(Permissions.WheelAdminPermissions.GetAwardType)]
        [ProducesResponseType(typeof(IEnumerable<EnumData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<EnumData>> GetAwardType()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new GeneralResult { IsSuccess = false, Message = localization.GetLocalizedString("UserNotLoggedIn") });
                }

                var enumValues = extendedBaseService.GetEnumValues<AwardType>();
                return Ok(enumValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the static content GetAwardType method.");
                return this.UnexpectedError("getting all Award type.");
            }
        }
         
        // ============= Wheel Play =============
        // ============= Wheel Play =============

        /// <summary>
        /// Retrieves all wheel play records with pagination for admins.
        /// </summary>
        [HttpGet("all-plays")]
        [RequiredPermission(Permissions.WheelAdminPermissions.ViewAllPlays)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<WheelPlayDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPlays([FromQuery] PaginationRequestDto pagination)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.GetAllUserPlaysAsync(pagination);
                if (result.IsSuccess == false) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all wheel plays.");
                return this.UnexpectedError("loading all plays");
            }
        }

        /// <summary>
        /// Updates the delivery status of a physical item spin.
        /// </summary>
        [HttpPut("delivery-status")]
        [RequiredPermission(Permissions.WheelAdminPermissions.ManageDelivery)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDeliveryStatus([FromForm] UpdateDeliveryStatusDto dto)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.MarkPlayDeliveredAsync(dto.PlayId, dto.IsDelivered);
                if (result.IsSuccess == false) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status for PlayId={PlayId}", dto.PlayId);
                return this.UnexpectedError("updating delivery status");
            }
        }

        /// <summary>
        /// Retrieves wheel plays with a specific delivery status (for physical items).
        /// </summary>
        [HttpGet("plays-by-delivery")]
        [RequiredPermission(Permissions.WheelAdminPermissions.ViewDeliveryStatus)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<WheelPlayDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPlaysByDeliveryStatus([FromQuery] bool delivered, [FromQuery] PaginationRequestDto pagination)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.GetPlaysByDeliveryStatusAsync(delivered, pagination);
                if (result.IsSuccess == false) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plays by delivery status.");
                return this.UnexpectedError("loading plays by delivery");
            }
        }

        /// <summary>
        /// Retrieves paginated physical item wheel plays across all users,
        /// optionally filtered by delivery status.
        /// </summary>
        /// <param name="isDelivered">true for delivered, false for undelivered, null for all.</param>
        /// <param name="pagination">Pagination parameters.</param>
        [HttpGet("physical-plays")]
        [RequiredPermission(Permissions.WheelAdminPermissions.ViewPhysicalDelivery)]
        [ProducesResponseType(typeof(GeneralResult<PagedResult<WheelPlayDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPhysicalItemPlays([FromQuery] bool? isDelivered, [FromQuery] PaginationRequestDto pagination)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.GetPhysicalItemPlaysByDeliveryStatusAsync(isDelivered, pagination);
                if (!result.IsSuccess) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving physical item plays.");
                return this.UnexpectedError("loading physical item plays");
            }
        }

        /// <summary>
        /// Updates the delivery status of a physical item award for a specific wheel play.
        /// </summary>
        [HttpPut("physical-delivery")]
        [RequiredPermission(Permissions.WheelAdminPermissions.ManagePhysicalDelivery)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePhysicalItemDelivery([FromForm] UpdateDeliveryStatusDto dto)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _wheelPlayerService.UpdatePhysicalItemDeliveryStatusAsync(dto.PlayId, dto.IsDelivered);
                if (result.IsSuccess == false) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating physical item delivery status. PlayId={PlayId}", dto.PlayId);
                return this.UnexpectedError("updating physical item delivery");
            }
        }
    }
}
