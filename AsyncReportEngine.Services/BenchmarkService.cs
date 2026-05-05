using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
using AsyncReportEngine.Shared.Dtos.Reports;

namespace AsyncReportEngine.Services;

public class BenchmarkService : IBenchmarkService
{
    private readonly IBenchmarkRepository benchmarkRepository;
    private readonly IReportRepository reportRepository;
    private readonly ISyncReportService syncReportService;
    private readonly IQueueService queueService;
    private readonly IInMemoryQueue inMemoryQueue;

    public BenchmarkService(
        IBenchmarkRepository benchmarkRepository,
        IReportRepository reportRepository,
        ISyncReportService syncReportService,
        IQueueService queueService,
        IInMemoryQueue inMemoryQueue)
    {
        this.benchmarkRepository = benchmarkRepository;
        this.reportRepository = reportRepository;
        this.syncReportService = syncReportService;
        this.queueService = queueService;
        this.inMemoryQueue = inMemoryQueue;
    }

    public async Task<BenchmarkRunDto> RunSyncBenchmarkAsync(BenchmarkRequestDto dto)
    {
        var runId = await benchmarkRepository.CreateRunAsync("Sync", dto.PartnerIds.Count);

        var result = await syncReportService.GenerateReportSyncAsync(dto.StartDate, dto.EndDate, dto.PartnerIds);

        await benchmarkRepository.CompleteRunAsync(runId, result.TotalDurationMs, result.AvgDurationMs, result.ThroughputPerSec);

        return new BenchmarkRunDto
        {
            Id = runId,
            Approach = "Sync",
            RequestedCount = dto.PartnerIds.Count,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            TotalDurationMs = result.TotalDurationMs,
            AvgDurationMs = result.AvgDurationMs,
            ThroughputPerSec = result.ThroughputPerSec
        };
    }
    public async Task<Guid> RunAsyncBenchmarkAsync(BenchmarkRequestDto dto, string userId)
    {
        var runId = await benchmarkRepository.CreateRunAsync(dto.Approach, dto.PartnerIds.Count);

        foreach (var partnerId in dto.PartnerIds)
        {
            var requestId = Guid.NewGuid();
            await reportRepository.CreateRequestWithRunAsync(requestId, userId, runId);

            var message = new ReportGenerationMessage
            {
                RequestId = requestId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UserEmail = dto.Email ?? "user@example.com",
                PartnerId = partnerId,
                BenchmarkRunId = runId
            };

            if (dto.Approach == "InMemory")
                await inMemoryQueue.EnqueueAsync(message);
            else
                await queueService.SendMessageAsync(message);
        }

        return runId;
    }

    public async Task<BenchmarkRunDto?> GetRunAsync(Guid runId)
    {
        var run = await benchmarkRepository.GetRunAsync(runId);
        if (run is null) return null;

        return new BenchmarkRunDto
        {
            Id = run.Id,
            Approach = run.Approach,
            RequestedCount = run.RequestedCount,
            Status = run.Status,
            CreatedAt = run.CreatedAt,
            CompletedAt = run.CompletedAt,
            TotalDurationMs = run.TotalDurationMs,
            AvgDurationMs = run.AvgDurationMs,
            ThroughputPerSec = run.ThroughputPerSec
        };
    }

    public async Task<List<BenchmarkRunDto>> GetHistoryAsync(int take = 20)
    {
        var runs = await benchmarkRepository.GetHistoryAsync(take);

        return runs.Select(r => new BenchmarkRunDto
        {
            Id = r.Id,
            Approach = r.Approach,
            RequestedCount = r.RequestedCount,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            CompletedAt = r.CompletedAt,
            TotalDurationMs = r.TotalDurationMs,
            AvgDurationMs = r.AvgDurationMs,
            ThroughputPerSec = r.ThroughputPerSec
        }).ToList();
    }
}