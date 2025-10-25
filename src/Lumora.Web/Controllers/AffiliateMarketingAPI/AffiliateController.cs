using Lumora.DataAnnotations;
using Lumora.DTOs.AffiliateMarketing;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.AffiliateMarketingIntf;

namespace Lumora.Web.Controllers.AffiliateMarketingAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AffiliateRoles)]
    public class AffiliateController(
        ILogger<AffiliateController> logger, AffiliateMessage messages, IAffiliateService affiliateService) : AuthenticatedController
    {
        private readonly ILogger<AffiliateController> _logger = logger;
        private readonly AffiliateMessage _messages = messages;
        private readonly IAffiliateService _affiliateService = affiliateService;

        /// <summary>
        /// Creates a new promo code for the current affiliate.
        /// </summary>
        [HttpPost("create")]
        [RequiredPermission(Permissions.PromoCodePermissions.Create)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] PromoCodeCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var modelCheck = this.ValidateModelState(_logger);
                if (modelCheck != null) return modelCheck;

                var result = await _affiliateService.CreatePromoCodeAsync(dto, CurrentUserId!, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.Conflict => StatusCode(409, result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PromoCodeController - Create: Unexpected error.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Create Promo Code"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Registers the usage of a promo code linked to a payment.
        /// </summary>
        [HttpPost("register-usage")]
        [RequiredPermission(Permissions.PromoCodePermissions.RegisterUsage)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUsage([FromQuery] int paymentId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _affiliateService.RegisterPromoCodeUsageAsync(paymentId, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.Conflict => StatusCode(409, result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PromoCodeController - RegisterUsage: Unexpected error.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Register Promo Code Usage"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Deactivates all active promo codes in the system.
        /// </summary>
        [HttpPost("deactivate-all")]
        [RequiredPermission(Permissions.PromoCodePermissions.DeactivateAll)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateAll(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _affiliateService.DeactivateAllPromoCodesAsync(CurrentUserId!, cancellationToken);

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
                _logger.LogError(ex, "PromoCodeController - DeactivateAll: Unexpected error.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Deactivate All Promo Codes"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves a full report of all promo codes with their usage statistics and status.
        /// </summary>
        [HttpGet("report")]
        [RequiredPermission(Permissions.PromoCodePermissions.ViewReport)]
        [ProducesResponseType(typeof(GeneralResult<List<PromoCodeReportDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReport(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _affiliateService.GetPromoCodeReportAsync(cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PromoCodeController - GetReport: Unexpected error.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Promo Code Report"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Retrieves all promo codes created by the current authenticated user (affiliate).
        /// </summary>
        [HttpGet("my-codes")]
        [RequiredPermission(Permissions.PromoCodePermissions.ViewOwn)]
        [ProducesResponseType(typeof(GeneralResult<List<PromoCodeReportDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyPromoCodes(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _affiliateService.GetPromoCodesByUserAsync(CurrentUserId!, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PromoCodeController - GetMyPromoCodes: Unexpected error.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Get My Promo Codes"), null, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Reactivates a previously deactivated promo code.
        /// </summary>
        [HttpPost("reactivate/{promoCodeId:int}")]
        [RequiredPermission(Permissions.PromoCodePermissions.Reactivate)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Reactivate([FromRoute] int promoCodeId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _affiliateService.ReactivatePromoCodeAsync(promoCodeId, CurrentUserId!, cancellationToken);

                return result.ErrorType switch
                {
                    ErrorType.Success => Ok(result),
                    ErrorType.BadRequest => BadRequest(result),
                    ErrorType.Conflict => StatusCode(409, result),
                    ErrorType.NotFound => NotFound(result),
                    ErrorType.InternalServerError => StatusCode(500, result),
                    _ => BadRequest(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PromoCodeController - Reactivate: Unexpected error.");
                return StatusCode(500, new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Reactivate Promo Code"), null, ErrorType.InternalServerError));
            }
        }
    }
}
