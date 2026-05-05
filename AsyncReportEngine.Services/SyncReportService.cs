using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    public async Task<List<string>> GenerateReportSyncAsync(DateTime startDate, DateTime endDate, List<int> customerIds)
    {
        logger.LogInformation("[SYNC] Запуск генерації для {Count} клієнтів", customerIds.Count);

        var tasks = customerIds.Select(id => ProcessSingleCustomerReportAsync(startDate, endDate, id));
        var results = await Task.WhenAll(tasks);

        logger.LogInformation("[SYNC] Завершено. Згенеровано {Count} звітів", results.Length);
        return results.ToList();
    }

    private async Task<string> ProcessSingleCustomerReportAsync(DateTime startDate, DateTime endDate, int customerId)
    {
        await semaphore.WaitAsync();
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IReportRepository>();
            var blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();

            logger.LogInformation("[SYNC] Початок обробки клієнта {CustomerId}", customerId);

            await Task.Delay(500);

            var orders = await repo.GetOrdersForReportAsync(startDate, endDate, customerId);
            var csv = BuildCsv(orders);

            var fileName = $"sync_report_cust_{customerId}_{Guid.NewGuid()}.csv";
            var fileUrl = await blobService.UploadReportAsync(fileName, csv);

            logger.LogInformation("[SYNC] Клієнт {CustomerId} готово. URL: {Url}", customerId, fileUrl);
            return fileUrl;
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