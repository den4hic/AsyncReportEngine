namespace AsyncReportEngine.Shared.Dtos.Reports;

public class SyncReportResult
{
    public List<string> FileUrls { get; set; } = [];
    public int TotalDurationMs { get; set; }
    public double AvgDurationMs { get; set; }
    public double ThroughputPerSec { get; set; }
}
