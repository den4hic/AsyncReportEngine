using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AsyncReportEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BenchmarkController : ControllerBase
{
    private readonly IBenchmarkService benchmarkService;

    public BenchmarkController(IBenchmarkService benchmarkService)
    {
        this.benchmarkService = benchmarkService;
    }

    [HttpPost]
    public async Task<IActionResult> RunBenchmark([FromBody] BenchmarkRequestDto dto)
    {
        if (dto.PartnerIds == null || dto.PartnerIds.Count == 0)
            return BadRequest("Вкажіть хоча б одного партнера.");

        if (dto.Approach == "Sync")
        {
            var result = await benchmarkService.RunSyncBenchmarkAsync(dto);
            return Ok(result);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous_user";
        var runId = await benchmarkService.RunAsyncBenchmarkAsync(dto, userId);

        return Accepted(new
        {
            RunId = runId,
            Approach = dto.Approach,
            ReportCount = dto.PartnerIds.Count,
            Status = "Running",
            Message = $"Benchmark запущено. Відстежуй статус через GET /api/benchmark/{runId}"
        });
    }

    [HttpGet("{runId}")]
    public async Task<IActionResult> GetRun(Guid runId)
    {
        var run = await benchmarkService.GetRunAsync(runId);
        if (run is null) return NotFound();
        return Ok(run);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int take = 20)
    {
        var runs = await benchmarkService.GetHistoryAsync(take);
        return Ok(runs);
    }
}