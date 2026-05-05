using AsyncReportEngine.Api.Hubs;
using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
using AsyncReportEngine.Shared.Entities;
using AsyncReportEngine.Shared.Enum;
using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace AsyncReportEngine.Api.BackgroundServices;

public class InMemoryReportWorker : BackgroundService
{
    private readonly IInMemoryQueue queue;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<InMemoryReportWorker> logger;
    private readonly IHubContext<ReportHub> hubContext;

    private const int BatchSize = 10;

    public InMemoryReportWorker(IInMemoryQueue queue, IServiceScopeFactory scopeFactory, ILogger<InMemoryReportWorker> logger, IHubContext<ReportHub> hubContext)
    {
        this.queue = queue;
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var runningTasks = new List<Task>();

        await foreach (var message in queue.DequeueAsync(stoppingToken))
        {
            runningTasks.Add(ProcessJobAsync(message));

            if (runningTasks.Count >= BatchSize)
            {
                var completed = await Task.WhenAny(runningTasks);
                runningTasks.Remove(completed);
            }
        }

        await Task.WhenAll(runningTasks);
    }

    private async Task ProcessJobAsync(ReportGenerationMessage jobData)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportRepository>();
        var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();

        logger.LogInformation("[IN-MEMORY] Початок генерації звіту {RequestId}", jobData.RequestId);
        await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Processing);

        var startedAt = DateTime.UtcNow;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        await Task.Delay(500);

        var orders = await repo.GetOrdersForReportAsync(jobData.StartDate, jobData.EndDate, jobData.PartnerId);
        var csv = BuildCsv(orders, jobData.PartnerId);

        var customerPrefix = jobData.PartnerId.HasValue ? $"customer_{jobData.PartnerId}_" : "all_";
        var fileName = $"in_memory_report_{customerPrefix}{jobData.RequestId}.csv";
        var fileUrl = await blobService.UploadReportAsync(fileName, csv);

        sw.Stop();
        await repo.UpdateTimingAsync(jobData.RequestId, startedAt, (int)sw.ElapsedMilliseconds);
        await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Completed, fileUrl: fileUrl);

        if (jobData.BenchmarkRunId.HasValue)
        {
            var benchmarkRepo = scope.ServiceProvider.GetRequiredService<IBenchmarkRepository>();
            await benchmarkRepo.TryCompleteRunAsync(jobData.BenchmarkRunId.Value);
        }

        await hubContext.Clients.All.SendAsync("ReportReady", new
        {
            RequestId = jobData.RequestId,
            FileUrl = fileUrl
        });

        logger.LogInformation("[IN-MEMORY] Звіт {RequestId} готовий. URL: {Url}", jobData.RequestId, fileUrl);
    }

    private static string BuildCsv(IEnumerable<Order> orders, int? partnerId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("OrderId,Date,Customer,TotalAmount,Status");

        foreach (var order in orders)
        {
            var status = order.Transactions != null && order.Transactions.Any() ? "Paid" : "Unpaid";
            var customer = order.Customer is not null
                ? $"{order.Customer.FirstName} {order.Customer.LastName}"
                : "Unknown";

            sb.AppendLine($"{order.Id},{order.OrderDate:yyyy-MM-dd},{customer},{order.TotalAmount},{status}");
        }

        return sb.ToString();
    }
}