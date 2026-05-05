namespace AsyncReportEngine.Shared.Dtos.Reports;

public class BenchmarkRunDto
{
    public Guid Id { get; set; }
    public string Approach { get; set; } = string.Empty;
    public int RequestedCount { get; set; }
    public int CompletedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? TotalDurationMs { get; set; }
    public double? AvgDurationMs { get; set; }
    public double? ThroughputPerSec { get; set; }
}
