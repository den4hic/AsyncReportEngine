using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Dashboards;
using AsyncReportEngine.Shared.Enum;

namespace AsyncReportEngine.Services;

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository orderRepository;
    private readonly IPartnerRepository partnerRepository;
    private readonly IReportRepository reportRepository;

    public DashboardService(IOrderRepository orderRepository, IPartnerRepository partnerRepository, IReportRepository reportRepository)
    {
        this.orderRepository = orderRepository;
        this.partnerRepository = partnerRepository;
        this.reportRepository = reportRepository;
    }

    public async Task<DashboardSummaryDto> GetSystemSummaryAsync()
    {
        var totalOrdersTask = await orderRepository.GetTotalCountAsync();
        var totalPartnersTask = await  partnerRepository.GetTotalCountAsync();
        var statusCountsTask = await reportRepository.GetRequestsStatusCountsAsync();


        var statuses = statusCountsTask;

        return new DashboardSummaryDto
        {
            TotalOrders = totalOrdersTask,
            TotalPartners = totalPartnersTask,
            ReportsCompleted = statuses.GetValueOrDefault(ReportStatus.Completed.ToString(), 0),
            ReportsPending = statuses.GetValueOrDefault(ReportStatus.Pending.ToString(), 0) + statuses.GetValueOrDefault(ReportStatus.Processing.ToString(), 0),
            ReportsFailed = statuses.GetValueOrDefault(ReportStatus.Failed.ToString(), 0)
        };
    }
}
