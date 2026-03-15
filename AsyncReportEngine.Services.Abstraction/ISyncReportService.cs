namespace AsyncReportEngine.Services.Abstraction;

public interface ISyncReportService
{
    Task<List<string>> GenerateReportSyncAsync(DateTime startDate, DateTime endDate, List<int> customerIds);
}
