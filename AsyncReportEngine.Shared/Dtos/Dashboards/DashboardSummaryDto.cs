namespace AsyncReportEngine.Shared.Dtos.Dashboards;

public class DashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public int ReportsCompleted { get; set; }
    public int ReportsPending { get; set; }
    public int ReportsFailed { get; set; }
    public int TotalPartners { get; set; }
}
