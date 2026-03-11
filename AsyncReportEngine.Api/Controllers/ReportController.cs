using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services;
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
    private readonly ISyncReportService syncReportService;

    public ReportsController(IReportRepository repository, IQueueService queueService, ISyncReportService syncReportService)
    {
        this.repository = repository;
        this.queueService = queueService;
        this.syncReportService = syncReportService;
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

    [HttpPost("sync-request")]
    public async Task<IActionResult> RequestReportSync([FromBody] CreateReportDto request)
    {
        try
        {
            var filePath = await syncReportService.GenerateReportSyncAsync(request.StartDate, request.EndDate);

            return Ok(new
            {
                Message = "Звіт успішно згенеровано синхронно!",
                File = filePath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Помилка генерації: {ex.Message}");
        }
    }

    [HttpPost("bulk-request")]
    public async Task<IActionResult> RequestBulkReports([FromBody] CreateReportDto dto)
    {
        if (dto.PartnerIds == null || !dto.PartnerIds.Any())
            return BadRequest("Вкажіть хоча б одного партнера (PartnerIds).");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous_user";
        var generatedRequests = new List<object>();

        foreach (var partnerId in dto.PartnerIds)
        {
            var requestId = Guid.NewGuid();

            await repository.CreateRequestAsync(requestId, userId);

            var message = new ReportGenerationMessage
            {
                RequestId = requestId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UserEmail = dto.Email ?? "user@example.com",
                PartnerId = partnerId
            };

            await queueService.SendMessageAsync(message);

            generatedRequests.Add(new { RequestId = requestId, PartnerId = partnerId, Status = "Pending" });
        }

        return Accepted(new
        {
            Message = $"Успішно додано {dto.PartnerIds.Count} задач у чергу.",
            Tasks = generatedRequests
        });
    }
}