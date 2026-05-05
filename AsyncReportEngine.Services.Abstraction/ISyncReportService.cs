using AsyncReportEngine.Shared.Dtos.Reports;

namespace AsyncReportEngine.Services.Abstraction;

public interface ISyncReportService
{
    Task<SyncReportResult> GenerateReportSyncAsync(DateTime startDate, DateTime endDate, List<int> customerIds);
}
