using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
using AsyncReportEngine.Shared.Dtos.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AsyncReportEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository repository;
    private readonly IQueueService queueService;

    public ReportsController(IReportRepository repository, IQueueService queueService)
    {
        this.repository = repository;
        this.queueService = queueService;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestReport([FromBody] CreateReportDto dto)
    {
        if (dto.StartDate > dto.EndDate)
        {
            return BadRequest("Start date should be before end date.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous_user";

        var requestId = Guid.NewGuid();

        await repository.CreateRequestAsync(requestId, userId);

        var message = new ReportGenerationMessage
        {
            RequestId = requestId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            UserEmail = dto.Email ?? "user@example.com"
        };

        await queueService.SendMessageAsync(message);

        return Accepted(new { RequestId = requestId, Status = "Pending", Message = "Report is generating." });
    }

    [HttpGet("{requestId}")]
    public async Task<IActionResult> GetStatus(Guid requestId)
    {
        var request = await repository.GetRequestByIdAsync(requestId);

        if (request == null)
        {
            return NotFound("Request was not found");
        }

        return Ok(new
        {
            request.Id,
            Status = request.Status.ToString(),
            request.CreatedAt,
            request.FinishedAt,
            request.FileUrl,
            request.ErrorMessage
        });
    }
}