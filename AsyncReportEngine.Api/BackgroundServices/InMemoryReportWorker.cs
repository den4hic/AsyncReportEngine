using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
using AsyncReportEngine.Shared.Enum;
using System.Text;

namespace AsyncReportEngine.Api.BackgroundServices;

public class InMemoryReportWorker : BackgroundService
{
    private readonly IInMemoryQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InMemoryReportWorker> _logger;

    public InMemoryReportWorker(IInMemoryQueue queue, IServiceScopeFactory scopeFactory, ILogger<InMemoryReportWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("--> [IN-MEMORY WORKER] Запущено. Очікування задач...");

        await foreach (var message in _queue.DequeueAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[IN-MEMORY WORKER] Помилка обробки задачі {message.RequestId}");
            }
        }
    }

    private async Task ProcessJobAsync(ReportGenerationMessage jobData)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportRepository>();
        var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();

        _logger.LogInformation($"[IN-MEMORY WORKER] Початок генерації звіту {jobData.RequestId}");
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
        _logger.LogInformation($"[IN-MEMORY WORKER] Звіт {jobData.RequestId} успішно згенеровано! URL: {fileUrl}");
    }
}
