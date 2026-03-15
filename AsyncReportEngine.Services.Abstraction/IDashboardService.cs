using AsyncReportEngine.Shared.Dtos.Dashboards;

namespace AsyncReportEngine.Services.Abstraction;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSystemSummaryAsync();
}
