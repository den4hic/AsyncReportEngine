using AsyncReportEngine.Shared.Dtos.Reports;

namespace AsyncReportEngine.Services.Abstraction;

public interface IReportHistoryService
{
    Task<IEnumerable<ReportHistoryDto>> GetUserHistoryAsync(string userId, int take);
}
