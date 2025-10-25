//using Lumora.DTOs.Email;

//namespace Lumora.Web.Controllers.Email;

//[Authorize(Roles = "Admin")]
//[Route("api/[controller]")]
//[ApiController]
//public class EmailSendController : ControllerBase
//{
//    private readonly IEmailService _emailService;
//    public EmailSendController(IEmailService emailService)
//    {
//        _emailService = emailService;
//    }

//    /// <summary>
//    /// Send an email using the provided request.
//    /// </summary>
//    [HttpPost("send")]
//    [SwaggerOperation(Tags = new[] { "Email" })]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
//    {
//        try
//        {
//            var messageId = await _emailService.SendAsync(request.Subject, request.FromEmail, request.FromName, request.Recipients, request.Body, request.Attachments);

//            return Ok(new { MessageId = messageId });
//        }
//        catch (Exception ex)
//        {
//            return BadRequest(new { Error = ex.Message });
//        }
//    }
//}
