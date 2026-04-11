using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using System.Text;

namespace AsyncReportEngine.Services;

public class SyncReportService : ISyncReportService
{
    private readonly IReportRepository reportRepository;
    private readonly IBlobService blobService;

    public SyncReportService(IReportRepository reportRepository, IBlobService blobService)
    {
        this.reportRepository = reportRepository;
        this.blobService = blobService;
    }

    public async Task<List<string>> GenerateReportSyncAsync(DateTime startDate, DateTime endDate, List<int> customerIds)
    {
        var tasks = customerIds.Select(async customerId =>
        {
            return await ProcessSingleCustomerReportAsync(startDate, endDate, customerId);
        });

        var results = await Task.WhenAll(tasks);

        return results.ToList();
    }

    private async Task<string> ProcessSingleCustomerReportAsync(DateTime startDate, DateTime endDate, int customerId)
    {
        Fibonacci(40);

        var orders = await reportRepository.GetOrdersForReportAsync(startDate, endDate, customerId);

        var sb = new StringBuilder();
        sb.AppendLine("OrderId,Date,Customer,TotalAmount,Status");

        foreach (var order in orders)
        {
            var status = order.Transactions.Any() ? "Paid" : "Unpaid";
            var line = $"{order.Id},{order.OrderDate:yyyy-MM-dd},{order.Customer?.FirstName} {order.Customer?.LastName},{order.TotalAmount},{status}";
            sb.AppendLine(line);
        }

        var fileName = $"sync_report_cust_{customerId}_{Guid.NewGuid()}.csv";
        return await blobService.UploadReportAsync(fileName, sb.ToString());
    }

    private long Fibonacci(int n)
    {
        if (n <= 1) return n;
        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }
}