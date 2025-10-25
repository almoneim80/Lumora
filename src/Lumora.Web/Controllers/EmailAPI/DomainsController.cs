//using System;
//using Lumora.Controllers;

//namespace Lumora.Web.Controllers.Email;

//[Authorize(Roles = "Admin")]
//[Route("api/[controller]")]
//public class DomainsController : BaseController<Domain, DomainCreateDto, DomainUpdateDto, DomainDetailsDto, DomainExpotDto>
//{
//    private readonly IDomainService domainService;

//    public DomainsController(
//        BaseService<Domain, DomainCreateDto, DomainUpdateDto, DomainDetailsDto> service,
//        IDomainService domainService,
//        ILocalizationManager? localization,
//        ILogger<DomainsController> logger)
//        : base(service, localization, logger)
//    {
//        this.domainService = domainService;
//    }

//    public bool IsDeleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//    public DateTimeOffset? DeletedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//    /// <summary>
//    /// Verify domain name.
//    /// </summary>
//    [HttpGet("verify/{name}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<DomainDetailsDto>> Verify(string name, bool force = false)
//    {
//        var result = await domainService.VerifyDomainAsync(name, force);
//        return Ok(result);
//    }

//    protected async Task SaveRangeAsync(List<Domain> newRecords)
//    {
//        await domainService.SaveRangeAsync(newRecords);
//    }
//}
