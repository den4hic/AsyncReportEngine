using AsyncReportEngine.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace AsyncReportEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ICatalogService catalogService;

    public DashboardController(ICatalogService catalogService)
    {
        this.catalogService = catalogService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await catalogService.GetDashboardStatsAsync();
        return Ok(stats);
    }
}
