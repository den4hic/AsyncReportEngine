using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class BenchmarkRepository : IBenchmarkRepository
{
    private readonly ReportDbContext context;

    public BenchmarkRepository(ReportDbContext context)
    {
        this.context = context;
    }

    public async Task<Guid> CreateRunAsync(string approach, int requestedCount)
    {
        var run = new BenchmarkRun
        {
            Id = Guid.NewGuid(),
            Approach = approach,
            RequestedCount = requestedCount,
            Status = "Running",
            CreatedAt = DateTime.UtcNow
        };

        context.BenchmarkRuns.Add(run);
        await context.SaveChangesAsync();
        return run.Id;
    }

    public async Task<BenchmarkRun?> GetRunAsync(Guid runId)
    {
        return await context.BenchmarkRuns.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == runId);
    }

    public async Task<List<BenchmarkRun>> GetHistoryAsync(int take = 20)
    {
        return await context.BenchmarkRuns.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task TryCompleteRunAsync(Guid runId)
    {
        var run = await context.BenchmarkRuns.FindAsync(runId);
        if (run is null || run.Status == "Completed") return;

        var completedCount = await context.ReportRequests
            .CountAsync(r => r.BenchmarkRunId == runId && r.Status == Shared.Enum.ReportStatus.Completed);

        if (completedCount < run.RequestedCount) return;

        var reports = await context.ReportRequests
            .Where(r => r.BenchmarkRunId == runId && r.StartedAt != null && r.DurationMs != null)
            .ToListAsync();

        var firstStart = reports.Min(r => r.StartedAt!.Value);
        var lastFinish = reports.Max(r => r.FinishedAt ?? DateTime.UtcNow);
        var totalMs = (int)(lastFinish - firstStart).TotalMilliseconds;
        var avgMs = reports.Average(r => r.DurationMs!.Value);
        var throughput = totalMs > 0 ? reports.Count / (totalMs / 1000.0) : 0;

        run.Status = "Completed";
        run.CompletedAt = DateTime.UtcNow;
        run.TotalDurationMs = totalMs;
        run.AvgDurationMs = Math.Round(avgMs, 2);
        run.ThroughputPerSec = Math.Round(throughput, 2);

        await context.SaveChangesAsync();
    }
}