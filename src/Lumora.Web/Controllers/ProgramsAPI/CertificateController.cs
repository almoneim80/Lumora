using Lumora.DataAnnotations;
using Lumora.Infrastructure.PermissionInfra;
using Lumora.Interfaces.PrograProgramIntfms;
namespace Lumora.Web.Controllers.ProgramsAPI
{
    [ApiController]
    [Route("wejha/api/[controller]")]
    [Authorize(Roles = AppRoles.CertificateRoles)]
    public class CertificateController(
        ICertificateService certificateService,
        CertificateMessages messages,
        ILogger<CertificateController> logger) : AuthenticatedController
    {
        private readonly ICertificateService _certificateService = certificateService;
        private readonly ILogger<CertificateController> _logger = logger;

        /// <summary>
        /// Initiates the certificate issuance process for a specified enrollment record.
        /// </summary>
        /// <param name="enrollmentId">The unique identifier of the enrollment record for which the certificate is to be issued.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation if needed.</param>
        [HttpPost("Issu/")]
        [RequiredPermission(Permissions.CertificatePermissions.Issue)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Issu([FromQuery] int enrollmentId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _certificateService.IssueCertificateAsync(enrollmentId, cancellationToken);
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
                _logger.LogError(ex, "An Unexpected error has occurred while issuing certificate.");
                return StatusCode(500,
                    new GeneralResult { IsSuccess = false, Message = messages.GetUnexpectedErrorMessage(" issuing certificate."), Data = null });
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific certificate using its unique identifier.
        /// </summary>
        /// <param name="certificateId">The unique identifier of the certificate to retrieve.</param>
        /// <param name="cancellationToken">Token to signal cancellation of the request.</param>
        [HttpGet("{certificateId:int}")]
        [RequiredPermission(Permissions.CertificatePermissions.ViewById)]
        [ProducesResponseType(typeof(GeneralResult<ProgramCertificateDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] int certificateId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _certificateService.GetByIdAsync(certificateId, cancellationToken);
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
                _logger.LogError(ex, "CertificateController - GetById : Unexpected error retrieving certificate.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("retrieving certificate"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves a list of certificates associated with the currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation if requested.</param>
        [HttpGet("user-certificates")]
        [RequiredPermission(Permissions.CertificatePermissions.ViewUserCertificates)]
        [ProducesResponseType(typeof(GeneralResult<List<ProgramCertificateListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserCertificates(CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _certificateService.GetUserCertificatesAsync(CurrentUserId!, cancellationToken);
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
                _logger.LogError(ex, "CertificateController - GetUserCertificates : Unexpected error retrieving user certificates.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("retrieving user certificates"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Exports a certificate as a downloadable PDF file.
        /// </summary>
        /// <param name="certificateId">The unique identifier of the certificate to export.</param>
        /// <param name="cancellationToken">Token used to observe request cancellation.</param>
        [HttpGet("export-pdf")]
        [RequiredPermission(Permissions.CertificatePermissions.ExportPdf)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportCertificatePdf([FromQuery] int certificateId, CancellationToken cancellationToken)
        {
            try
            {
                var userCheck = CheckUserOrUnauthorized();
                if (userCheck != null) return userCheck;

                var result = await _certificateService.ExportCertificatePdfAsync(certificateId, cancellationToken);
                if (!result.IsSuccess || result.Data == null)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        ErrorType.InternalServerError => StatusCode(500, result),
                        _ => BadRequest(result)
                    };
                }

                return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CertificateController - ExportCertificatePdf : Unexpected error exporting certificate PDF.");
                return StatusCode(500, new GeneralResult
                {
                    IsSuccess = false,
                    Message = messages.GetUnexpectedErrorMessage("exporting certificate PDF"),
                    Data = null
                });
            }
        }

        /// <summary>
        /// Verifies the validity and authenticity of a certificate using a public verification code.
        /// </summary>
        /// <param name="code">The public verification code associated with the certificate.</param>
        /// <param name="cancellationToken">Token to monitor cancellation requests.</param>
        [AllowAnonymous]
        [HttpGet("verify")]
        [ProducesResponseType(typeof(GeneralResult<ProgramCertificateDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GeneralResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyCertificate([FromQuery] string code, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _certificateService.VerifyCertificateAsync(code, cancellationToken);
                if (!result.IsSuccess)
                {
                    return result.ErrorType switch
                    {
                        ErrorType.BadRequest => BadRequest(result),
                        ErrorType.NotFound => NotFound(result),
                        _ => StatusCode(500, result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CertificateController - VerifyCertificate : Unexpected error.");
                return StatusCode(500,
                    new GeneralResult
                    {
                        IsSuccess = false,
                        Message = messages.GetUnexpectedErrorMessage("verifying certificate"),
                        Data = null
                    });
            }
        }
    }
}
