using AsyncReportEngine.Shared.Entities;
using AsyncReportEngine.Shared.Enum;

namespace AsyncReportEngine.DataAccess.Abstraction.Repositories;

public interface IReportRepository
{
    Task<Guid> CreateRequestAsync(Guid requestId, string userId);
    Task UpdateStatusAsync(Guid requestId, ReportStatus status, string? fileUrl = null, string? error = null);
    Task<ReportRequest?> GetRequestByIdAsync(Guid requestId);
    Task<List<ReportRequest>> GetUserRequestsAsync(string userId);

    Task<List<Order>> GetOrdersForReportAsync(DateTime startDate, DateTime endDate);

    Task<List<Order>> GetOrdersForReportAsync(DateTime start, DateTime end, int? partnerId = null);

    Task<IEnumerable<ReportRequest>> GetUserRequestsHistoryAsync(string userId, int take);
    Task<Dictionary<string, int>> GetRequestsStatusCountsAsync();

    Task<(List<ReportRequest> Items, int TotalCount)> GetRequestsPagedAsync(int skip, int take, ReportStatus? status);
    Task<Dictionary<ReportStatus, int>> GetRequestsStatsAsync();
    Task UpdateTimingAsync(Guid requestId, DateTime startedAt, int durationMs);
}