using AsyncReportEngine.Shared.Dtos.Pagination;
using AsyncReportEngine.Shared.Dtos.Reports;

namespace AsyncReportEngine.Services.Abstraction;

public interface IReportHistoryService
{
    Task<PagedResult<ReportHistoryDto>> GetHistoryPagedAsync(int page, int pageSize, string? status);
    Task<ReportStatsDto> GetStatsAsync();
    Task<IEnumerable<ReportHistoryDto>> GetUserHistoryAsync(string userId, int take);
}
