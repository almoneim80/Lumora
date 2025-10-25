//using Lumora.DTOs.Email;

//namespace Lumora.Web.Controllers.Email;

//[AllowAnonymous]
//[Route("api/[controller]")]
//public class EmailController : ControllerBase
//{
//    private readonly IEmailVerifyService _emailVerifyService;
//    private readonly IMapper _mapper;
//    private readonly IEmailVerificationService _emailVerificationService;

//    public EmailController(
//        IEmailVerifyService emailVerifyService,
//        IMapper mapper,
//        IEmailVerificationService emailVerificationService)
//    {
//        _emailVerifyService = emailVerifyService;
//        _mapper = mapper;
//        _emailVerificationService = emailVerificationService;
//    }

//    /// <summary>
//    /// verify email address Domain.
//    /// </summary>
//    [HttpGet("verify-domain/{email}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult> VerifyEmailDomain([EmailAddress] string email)
//    {
//        var resultedDomainData = await _emailVerifyService.Verify(email);
//        var resultConverted = _mapper.Map<EmailVerifyDetailsDto>(resultedDomainData);
//        resultConverted.EmailAddress = email;

//        return Ok(resultConverted);
//    }

//    /// <summary>
//    /// Verify the user's email using a verification link.
//    /// </summary>
//    [HttpGet("ConfirmEmail")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> ConfirmEmail(string userId, string token)
//    {
//        var (succeeded, message, errors) = await _emailVerificationService.ConfirmEmailAsync(userId, token);

//        if (succeeded)
//        {
//            return Ok(new { Message = message });
//        }
//        else if (errors != null)
//        {
//            return BadRequest(new { Message = message, Errors = errors });
//        }
//        else
//        {
//            return BadRequest(new { Message = message });
//        }
//    }

//    /// <summary>
//    /// resend verification link.
//    /// </summary>
//    [HttpPost("resend/verification-link/{email}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> ResendVerificationLink(string email)
//    {
//        var (succeeded, message) = await _emailVerificationService.ResendVerificationLinkAsync(email);

//        if (succeeded)
//        {
//            return Ok(new { Message = message });
//        }
//        else
//        {
//            return BadRequest(new { Message = message });
//        }
//    }

//    /// <summary>
//    /// Verify the user's email using a verification Code.
//    /// </summary>
//    [HttpPost("verify-otp/{id}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> VerifyOtp(string id, [FromBody] string code)
//    {
//        var isValid = await _emailVerificationService.VerifyOtpAsync(id, code);

//        if (isValid)
//        {
//            return Ok(new { Message = "OTP Verified successfully, email confirmed!" });
//        }

//        return BadRequest(new { Message = "Invalid or expired OTP" });
//    }

//    /// <summary>
//    /// regenerate code for user.
//    /// </summary>
//    [HttpPost("regenerate-otp/{userId}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> RegenerateOtp(string userId)
//    {
//        var (succeeded, message, expireAt) = await _emailVerificationService.RegenerateOtpAsync(userId);

//        if (!succeeded)
//        {
//            return BadRequest(new { Message = message });
//        }

//        if (expireAt == null)
//        {
//            return Ok(new { Message = message });
//        }

//        return Ok(new { Message = message });
//    }
//}
