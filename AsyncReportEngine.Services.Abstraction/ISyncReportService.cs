namespace AsyncReportEngine.Services.Abstraction;

public interface ISyncReportService
{
    Task<string> GenerateReportSyncAsync(DateTime startDate, DateTime endDate);
}
