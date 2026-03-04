using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using System.Text;

namespace AsyncReportEngine.Services;

public class SyncReportService : ISyncReportService
{
    private readonly IReportRepository reportRepository;

    public SyncReportService(IReportRepository reportRepository)
    {
        this.reportRepository = reportRepository;
    }

    public async Task<string> GenerateReportSyncAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await reportRepository.GetOrdersForReportAsync(startDate, endDate);

        await Task.Delay(5000);

        var sb = new StringBuilder();
        sb.AppendLine("OrderId,Date,Customer,TotalAmount,Status");

        foreach (var order in orders)
        {
            var status = order.Transactions.Any() ? "Paid" : "Unpaid";
            var line = $"{order.Id},{order.OrderDate:yyyy-MM-dd},{order.Customer?.FirstName} {order.Customer?.LastName},{order.TotalAmount},{status}";
            sb.AppendLine(line);
        }

        var fileName = $"sync_report_{Guid.NewGuid()}.csv";
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedReports");
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        await File.WriteAllTextAsync(filePath, sb.ToString());

        return filePath;
    }
}
