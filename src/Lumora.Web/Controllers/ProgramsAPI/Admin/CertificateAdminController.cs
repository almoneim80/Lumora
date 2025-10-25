using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.PrograProgramIntfms;

namespace Lumora.Web.Controllers.ProgramsAPI.Admin
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.AllAdmins)]
    public class CertificateAdminController(
            ICertificateService certificateService,
            CertificateMessages messages,
            ILogger<CertificateController> logger) : AuthenticatedController
    {
        private readonly ICertificateService _certificateService = certificateService;
        private readonly ILogger<CertificateController> _logger = logger;

        /// <summary>
        /// Retrieves the total number of certificates issued for a specific training program.
        /// </summary>
        /// <param name="programId">The unique identifier of the training program.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        [HttpGet("count-by-program")]
        [RequiredPermission(Permissions.CertificateAdminPermissions.CountByProgram)]
        [ProducesResponseType(typeof(GeneralResult<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CountByProgram([FromQuery] int programId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _certificateService.CountProgramCertificatesAsync(programId, cancellationToken);
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
                _logger.LogError(ex, "CertificateController - CountByProgram : Unexpected error while counting certificates.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("counting program certificates"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Revokes an issued certificate with a provided reason.
        /// </summary>
        /// <param name="certificateId">The unique identifier of the certificate to be revoked.</param>
        /// <param name="reason">The reason for revoking the certificate. Should be specific and audit-friendly.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if required.</param>
        [HttpPost("revoke")]
        [RequiredPermission(Permissions.CertificateAdminPermissions.Revoke)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Revoke([FromQuery] int certificateId, [FromQuery] string reason, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _certificateService.RevokeCertificateAsync(certificateId, reason, cancellationToken);
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
                _logger.LogError(ex, "CertificateController - Revoke : Unexpected error revoking certificate.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("revoking certificate"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Generates a public verification code for a given certificate to enable external validation.
        /// </summary>
        /// <param name="certificateId">The unique identifier of the certificate for which to generate the verification code.</param>
        /// <param name="cancellationToken">Token for monitoring cancellation requests.</param>
        [HttpPost("generate-verification-code")]
        [RequiredPermission(Permissions.CertificateAdminPermissions.GenerateVerificationCode)]
        [ProducesResponseType(typeof(GeneralResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateVerificationCode([FromQuery] int certificateId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _certificateService.GeneratePublicVerificationCodeAsync(certificateId, cancellationToken);
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
                _logger.LogError(ex, "CertificateController - GenerateVerificationCode : Unexpected error generating verification code.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("generating verification code"),
                    Data = null
                });
            }
        }
    }
}
