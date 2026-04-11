namespace AsyncReportEngine.Shared.Dtos.Reports;

public class ReportStatsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int CompletedReports { get; set; }
    public int FailedReports { get; set; }
}
