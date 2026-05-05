using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Reports;
using AsyncReportEngine.Shared.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace AsyncReportEngine.Services;

public class SyncReportService : ISyncReportService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<SyncReportService> logger;
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(10);

    public SyncReportService(IServiceScopeFactory scopeFactory, ILogger<SyncReportService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    public async Task<SyncReportResult> GenerateReportSyncAsync(DateTime startDate, DateTime endDate, List<int> customerIds)
    {
        logger.LogInformation("[SYNC] Запуск генерації для {Count} клієнтів", customerIds.Count);

        var totalSw = Stopwatch.StartNew();

        var tasks = customerIds.Select(id => ProcessSingleCustomerReportAsync(startDate, endDate, id));
        var results = await Task.WhenAll(tasks);

        totalSw.Stop();

        var totalMs = (int)totalSw.ElapsedMilliseconds;
        var avgMs = results.Length > 0 ? results.Average(r => r.DurationMs) : 0;
        var throughput = totalMs > 0 ? results.Length / (totalMs / 1000.0) : 0;

        logger.LogInformation("[SYNC] Завершено {Count} звітів за {TotalMs}ms. Throughput: {Throughput:F2}/s",
            results.Length, totalMs, throughput);

        return new SyncReportResult
        {
            FileUrls = results.Select(r => r.FileUrl).ToList(),
            TotalDurationMs = totalMs,
            AvgDurationMs = Math.Round(avgMs, 2),
            ThroughputPerSec = Math.Round(throughput, 2)
        };
    }

    private async Task<(string FileUrl, int DurationMs)> ProcessSingleCustomerReportAsync(DateTime startDate, DateTime endDate, int customerId)
    {
        await semaphore.WaitAsync();
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IReportRepository>();
            var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();

            var sw = Stopwatch.StartNew();

            logger.LogInformation("[SYNC] Початок обробки клієнта {CustomerId}", customerId);

            await Task.Delay(500);

            var orders = await repo.GetOrdersForReportAsync(startDate, endDate, customerId);
            var csv = BuildCsv(orders);

            var fileName = $"sync_report_cust_{customerId}_{Guid.NewGuid()}.csv";
            var fileUrl = await blobService.UploadReportAsync(fileName, csv);

            sw.Stop();

            logger.LogInformation("[SYNC] Клієнт {CustomerId} готово за {Ms}ms. URL: {Url}",
                customerId, sw.ElapsedMilliseconds, fileUrl);

            return (fileUrl, (int)sw.ElapsedMilliseconds);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static string BuildCsv(IEnumerable<Order> orders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("OrderId,Date,Customer,TotalAmount,Status");

        foreach (var order in orders)
        {
            var status = order.Transactions.Any() ? "Paid" : "Unpaid";
            var customer = order.Customer is not null
                ? $"{order.Customer.FirstName} {order.Customer.LastName}"
                : "Unknown";

            sb.AppendLine($"{order.Id},{order.OrderDate:yyyy-MM-dd},{customer},{order.TotalAmount},{status}");
        }

        return sb.ToString();
    }
}