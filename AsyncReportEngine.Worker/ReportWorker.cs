using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
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

    private static readonly HttpClient sharedHttpClient = new HttpClient();

    public ReportWorker(QueueClient queueClient, IServiceScopeFactory scopeFactory, ILogger<ReportWorker> logger)
    {
        this.queueClient = queueClient;
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("--> [WORKER PROJECT] Service is working...");
        await queueClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(
                maxMessages: 32,
                visibilityTimeout: TimeSpan.FromMinutes(2),
                cancellationToken: stoppingToken);

            if (messages.Length > 0)
            {
                logger.LogInformation($"[WORKER] Отримано {messages.Length} завдань. Починаю паралельне завантаження в Blob Storage...");

                var tasks = messages.Select(async message =>
                {
                    try
                    {
                        await ProcessReportJob(message.MessageText);
                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Помилка обробки: {message.MessageId}");
                    }
                });

                await Task.WhenAll(tasks);
                logger.LogInformation($"[WORKER] Пачка з {messages.Length} завдань завершена!");
            }
            else
            {
                //await Task.Delay(3000, stoppingToken);
            }
        }
    }

    private async Task ProcessReportJob(string base64Message)
    {
        var jsonBytes = Convert.FromBase64String(base64Message);
        var jsonString = Encoding.UTF8.GetString(jsonBytes);
        var jobData = JsonSerializer.Deserialize<ReportGenerationMessage>(jsonString);

        if (jobData == null) return;

        using (var scope = scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IReportRepository>();

            logger.LogInformation($"[WORKER] 1. Отримую дані для запиту {jobData.RequestId}...");
            await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Processing);

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                Fibonacci(40);
                var orders = await repo.GetOrdersForReportAsync(jobData.StartDate, jobData.EndDate, jobData.PartnerId);

                logger.LogInformation($"[WORKER] Дані отримано: {orders.Count} рядків. Починаю генерацію CSV...");

                var sb = new StringBuilder();
                sb.AppendLine("OrderId,Date,Customer,TotalAmount,Status");

                foreach (var order in orders)
                {
                    var status = order.Transactions.Any() ? "Paid" : "Unpaid";
                    var line = $"{order.Id},{order.OrderDate:yyyy-MM-dd},{order.Customer?.FirstName} {order.Customer?.LastName},{order.TotalAmount},{status}";
                    sb.AppendLine(line);
                }

                var fileName = $"report_{jobData.PartnerId}_{jobData.RequestId}.csv";

                var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();

                logger.LogInformation($"[WORKER] Починаю завантаження {fileName} в Azure Blob Storage...");

                var fileUrl = await blobService.UploadReportAsync(fileName, sb.ToString());

                stopwatch.Stop();
                var resultInfo = $"Звіт готовий! Розмір: {orders.Count} рядків. Час: {stopwatch.Elapsed.TotalSeconds:F2} сек.";

                await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Completed, fileUrl: fileUrl);

                try
                {
                    var apiUrl = $"https://localhost:7193/api/reports/{jobData.RequestId}/notify-ready";

                    var content = new StringContent($"{{\"fileUrl\": \"{fileUrl}\"}}", Encoding.UTF8, "application/json");

                    await sharedHttpClient.PostAsync(apiUrl, content);

                    logger.LogInformation($"[WORKER] Сигнал SignalR відправлено на API!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[WORKER] Не вдалося відправити сповіщення на API.");
                }

                logger.LogInformation($"[WORKER] {resultInfo} | URL: {fileUrl}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Помилка генерації звіту");
                await repo.UpdateStatusAsync(jobData.RequestId, ReportStatus.Failed, error: ex.Message);
            }
        }
    }

    private long Fibonacci(int n)
    {
        if (n <= 1) return n;
        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }
}