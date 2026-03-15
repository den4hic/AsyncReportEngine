using AsyncReportEngine.Services;
using AsyncReportEngine.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsyncReportEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly IPartnerService partnerService;

    public CustomerController(IPartnerService partnerService)
    {
        this.partnerService = partnerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPartners()
    {
        var partners = await partnerService.GetAllAsync();
        return Ok(partners);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPartnerById(int id)
    {
        var partner = await partnerService.GetByIdAsync(id);
        if (partner == null) return NotFound("Партнера не знайдено");

        return Ok(partner);
    }
}
