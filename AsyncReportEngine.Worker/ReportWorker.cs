using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
using AsyncReportEngine.Shared.Entities;
using AsyncReportEngine.Shared.Enum;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text;
using System.Text.Json;

namespace AsyncReportEngine.Worker;

public class ReportWorker : BackgroundService
{
    private readonly QueueClient queueClient;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<ReportWorker> logger;
    private readonly IConfiguration config;

    private static readonly HttpClient httpClient = new HttpClient();

    public ReportWorker(QueueClient queueClient, IServiceScopeFactory scopeFactory, ILogger<ReportWorker> logger, IConfiguration config)
    {
        this.queueClient = queueClient;
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[AZURE WORKER] Сервіс запущено");
        await queueClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(
                maxMessages: 10,
                visibilityTimeout: TimeSpan.FromMinutes(2),
                cancellationToken: stoppingToken);

            if (messages.Length == 0)
            {
                await Task.Delay(3000, stoppingToken);
                continue;
            }

            logger.LogInformation("[AZURE WORKER] Отримано {Count} повідомлень", messages.Length);

            var tasks = messages.Select(async message =>
            {
                try
                {
                    await ProcessReportJob(message.MessageText);
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[AZURE WORKER] Помилка обробки повідомлення {MessageId}", message.MessageId);
                }
            });

            await Task.WhenAll(tasks);
            logger.LogInformation("[AZURE WORKER] Пачка завершена");
        }
    }

    private async Task ProcessReportJob(string base64Message)
    {
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64Message));
        var jobData = JsonSerializer.Deserialize<ReportGenerationMessage>(json);
        if (jobData is null) return;

        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportRepository>();
        var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();

        logger.LogInformation("[AZURE WORKER] Початок обробки {RequestId}", jobData.RequestId);
        await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Processing);

        try
        {
            var startedAt = DateTime.UtcNow;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            await Task.Delay(500);

            var orders = await repo.GetOrdersForReportAsync(jobData.StartDate, jobData.EndDate, jobData.PartnerId);
            var csv = BuildCsv(orders);

            var fileName = $"report_{jobData.PartnerId}_{jobData.RequestId}.csv";
            var fileUrl = await blobService.UploadReportAsync(fileName, csv);

            sw.Stop();
            await repo.UpdateTimingAsync(jobData.RequestId, startedAt, (int)sw.ElapsedMilliseconds);
            await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Completed, fileUrl: fileUrl);

            if (jobData.BenchmarkRunId.HasValue)
            {
                var benchmarkRepo = scope.ServiceProvider.GetRequiredService<IBenchmarkRepository>();
                await benchmarkRepo.TryCompleteRunAsync(jobData.BenchmarkRunId.Value);
            }

            await NotifyApiAsync(jobData.RequestId, fileUrl);

            logger.LogInformation("[AZURE WORKER] Звіт {RequestId} готовий. URL: {Url}", jobData.RequestId, fileUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[AZURE WORKER] Помилка генерації звіту {RequestId}", jobData.RequestId);
            await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Failed, error: ex.Message);
        }
    }

    private async Task NotifyApiAsync(Guid requestId, string fileUrl)
    {
        try
        {
            var apiBase = config["ApiBaseUrl"] ?? "https://localhost:7193";
            var url = $"{apiBase}/api/reports/{requestId}/notify-ready";
            var body = new StringContent($"{{\"fileUrl\":\"{fileUrl}\"}}", Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, body);
            logger.LogInformation("[AZURE WORKER] SignalR нотифікацію надіслано");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[AZURE WORKER] Не вдалося надіслати нотифікацію");
        }
    }

    private static string BuildCsv(IEnumerable<Order> orders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("OrderId,Date,Customer,TotalAmount,Status");

        foreach (var order in orders)
        {
            var status = order.Transactions.Any() ? "Paid" : "Unpaid";
            var customer = $"{order.Customer?.FirstName} {order.Customer?.LastName}".Trim();
            sb.AppendLine($"{order.Id},{order.OrderDate:yyyy-MM-dd},{customer},{order.TotalAmount},{status}");
        }

        return sb.ToString();
    }
}