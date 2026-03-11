using AsyncReportEngine.Api.Hubs;
using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
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

    public InMemoryReportWorker(IInMemoryQueue queue, IServiceScopeFactory scopeFactory, ILogger<InMemoryReportWorker> logger, IHubContext<ReportHub> hubContext)
    {
        this.queue = queue;
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("--> [IN-MEMORY WORKER] Запущено. Очікування задач...");

        await foreach (var message in queue.DequeueAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[IN-MEMORY WORKER] Помилка обробки задачі {message.RequestId}");
            }
        }
    }

    private async Task ProcessJobAsync(ReportGenerationMessage jobData)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportRepository>();
        var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();

        logger.LogInformation($"[IN-MEMORY WORKER] Початок генерації звіту {jobData.RequestId}");
        await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Processing);

        var orders = await repo.GetOrdersForReportAsync(jobData.StartDate, jobData.EndDate, jobData.PartnerId);

        var sb = new StringBuilder();
        sb.AppendLine("OrderId,Date,Customer,TotalAmount,Status");
        foreach (var order in orders)
        {
            var status = order.Transactions != null && order.Transactions.Any() ? "Paid" : "Unpaid";
            var customerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : "Unknown";
            sb.AppendLine($"{order.Id},{order.OrderDate:yyyy-MM-dd},{customerName},{order.TotalAmount},{status}");
        }

        var customerPrefix = jobData.PartnerId.HasValue ? $"customer_{jobData.PartnerId}_" : "all_";
        var fileName = $"in_memory_report_{customerPrefix}{jobData.RequestId}.csv";

        var fileUrl = await blobService.UploadReportAsync(fileName, sb.ToString());

        await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Completed, fileUrl: fileUrl);

        await hubContext.Clients.All.SendAsync("ReportReady", new
        {
            RequestId = jobData.RequestId,
            FileUrl = fileUrl
        });

        logger.LogInformation($"[IN-MEMORY WORKER] Звіт {jobData.RequestId} успішно згенеровано! URL: {fileUrl}");
    }
}
