using AsyncReportEngine.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsyncReportEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        this.dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSystemSummary()
    {
        var summary = await dashboardService.GetSystemSummaryAsync();
        return Ok(summary);
    }
}